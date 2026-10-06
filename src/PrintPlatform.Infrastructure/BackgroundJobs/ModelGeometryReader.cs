using System.IO.Compression;
using System.Xml.Linq;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Infrastructure.BackgroundJobs;

/// <summary>Normalised result of parsing any supported mesh: absolute volume + AABB + triangle count.</summary>
public sealed record MeshGeometry(
    decimal VolumeCubicMm,
    decimal BoundingBoxXmm,
    decimal BoundingBoxYmm,
    decimal BoundingBoxZmm,
    int TriangleCount);

/// <summary>
/// Dispatches model geometry extraction by <see cref="FileFormat"/>. STL reuses the
/// existing <see cref="BinaryStlReader"/> (binary + ASCII); OBJ and 3MF are parsed here.
/// Volume is the absolute signed-tetrahedron sum and is only meaningful for watertight
/// (closed) meshes — which is exactly the input a printable model is expected to be.
/// </summary>
public static class ModelGeometryReader
{
    public static MeshGeometry Read(FileFormat format, Stream stream)
    {
        switch (format)
        {
            case FileFormat.Stl:
            {
                var stl = BinaryStlReader.Read(stream);
                return new MeshGeometry(
                    stl.VolumeCubicMm, stl.BoundingBoxXmm, stl.BoundingBoxYmm, stl.BoundingBoxZmm,
                    stl.TriangleCount);
            }
            case FileFormat.Obj:
                return ReadObj(stream);
            case FileFormat.ThreeMF:
                return ReadThreeMf(stream);
            default:
                throw new NotSupportedException($"Unsupported model format: {format}");
        }
    }

    // ── OBJ (Wavefront) ────────────────────────────────────────────────────
    private static MeshGeometry ReadObj(Stream stream)
    {
        var vertices = new List<double[]>();
        double signedVolume = 0;
        int triangleCount = 0;
        var min = new[] { double.MaxValue, double.MaxValue, double.MaxValue };
        var max = new[] { double.MinValue, double.MinValue, double.MinValue };

        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed[0] == '#') continue;

            if (trimmed.StartsWith("v ", StringComparison.Ordinal))
            {
                var v = ParseVertex(trimmed.Substring(2));
                vertices.Add(v);
                UpdateBounds(min, max, v);
            }
            else if (trimmed.StartsWith("f ", StringComparison.Ordinal))
            {
                var indices = ParseFaceIndices(trimmed.Substring(2));
                // Fan-triangulate n-gons (quads/polygons → triangles).
                for (int i = 1; i + 1 < indices.Count; i++)
                {
                    var a = vertices[indices[0] - 1];
                    var b = vertices[indices[i] - 1];
                    var c = vertices[indices[i + 1] - 1];
                    signedVolume += SignedTetraVolume(a, b, c);
                    triangleCount++;
                }
            }
        }

        if (vertices.Count == 0 || triangleCount == 0)
            throw new InvalidDataException("OBJ contains no triangulatable faces.");

        return Build(signedVolume, min, max, triangleCount);
    }

    // ── 3MF (OPC package) ──────────────────────────────────────────────────
    private static MeshGeometry ReadThreeMf(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

        var modelEntry = archive.Entries.FirstOrDefault(e =>
            e.FullName.EndsWith(".model", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException("3MF package has no model part.");

        using var modelStream = modelEntry.Open();
        var doc = XDocument.Load(modelStream);

        XNamespace ns = doc.Root?.Name.Namespace ?? XNamespace.None;

        var vertices = doc.Descendants(ns + "vertex")
            .Select(v => new[]
            {
                ParseInvariant((string?)v.Attribute("x") ?? "0"),
                ParseInvariant((string?)v.Attribute("y") ?? "0"),
                ParseInvariant((string?)v.Attribute("z") ?? "0"),
            })
            .ToList();

        var triangles = doc.Descendants(ns + "triangle").ToList();
        if (vertices.Count == 0 || triangles.Count == 0)
            throw new InvalidDataException("3MF model has no mesh geometry (component/build trees are not supported yet).");

        double signedVolume = 0;
        var min = new[] { double.MaxValue, double.MaxValue, double.MaxValue };
        var max = new[] { double.MinValue, double.MinValue, double.MinValue };

        foreach (var v in vertices)
            UpdateBounds(min, max, v);

        foreach (var t in triangles)
        {
            var i1 = int.Parse((string?)t.Attribute("v1") ?? "0", System.Globalization.CultureInfo.InvariantCulture);
            var i2 = int.Parse((string?)t.Attribute("v2") ?? "0", System.Globalization.CultureInfo.InvariantCulture);
            var i3 = int.Parse((string?)t.Attribute("v3") ?? "0", System.Globalization.CultureInfo.InvariantCulture);
            signedVolume += SignedTetraVolume(vertices[i1], vertices[i2], vertices[i3]);
        }

        return Build(signedVolume, min, max, triangles.Count);
    }

    // ── shared helpers ─────────────────────────────────────────────────────
    private static MeshGeometry Build(double signedVolume, double[] min, double[] max, int triangleCount) =>
        new(
            (decimal)Math.Abs(signedVolume),
            (decimal)(max[0] - min[0]),
            (decimal)(max[1] - min[1]),
            (decimal)(max[2] - min[2]),
            triangleCount);

    private static double SignedTetraVolume(double[] a, double[] b, double[] c)
    {
        var cross0 = b[1] * c[2] - b[2] * c[1];
        var cross1 = b[2] * c[0] - b[0] * c[2];
        var cross2 = b[0] * c[1] - b[1] * c[0];
        return (a[0] * cross0 + a[1] * cross1 + a[2] * cross2) / 6.0;
    }

    private static void UpdateBounds(double[] min, double[] max, double[] v)
    {
        for (int i = 0; i < 3; i++)
        {
            if (v[i] < min[i]) min[i] = v[i];
            if (v[i] > max[i]) max[i] = v[i];
        }
    }

    private static double[] ParseVertex(string payload)
    {
        var parts = payload.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            throw new InvalidDataException("Malformed vertex line in OBJ.");
        return new[]
        {
            ParseInvariant(parts[0]),
            ParseInvariant(parts[1]),
            ParseInvariant(parts[2]),
        };
    }

    /// <summary>Parses 1-based face indices, stripping any "v/vt/vn" suffixes.</summary>
    private static List<int> ParseFaceIndices(string payload)
    {
        var indices = new List<int>();
        foreach (var token in payload.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = token.Split('/')[0];
            if (int.TryParse(idx, out var parsed) && parsed != 0)
            {
                if (parsed < 0)
                    throw new InvalidDataException("Negative OBJ indices are not supported.");
                indices.Add(parsed);
            }
        }
        return indices;
    }

    private static double ParseInvariant(string s) =>
        double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
}