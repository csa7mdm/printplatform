namespace PrintPlatform.Infrastructure.BackgroundJobs;

/// <summary>Result of parsing an STL mesh: signed volume + axis-aligned bounding box.</summary>
public sealed record StlGeometry(
    decimal VolumeCubicMm,
    decimal BoundingBoxXmm,
    decimal BoundingBoxYmm,
    decimal BoundingBoxZmm,
    int TriangleCount);

/// <summary>
/// Minimal binary STL parser. Computes mesh volume via the signed-tetrahedron method
/// (sum of signed volumes of tetrahedra formed by each triangle and the origin) and the
/// axis-aligned bounding box. Falls back to ASCII parsing when the file is not binary.
/// </summary>
public static class BinaryStlReader
{
    private const int HeaderSize = 80;

    /// <summary>Reads an STL stream and returns its geometry. Throws on malformed input.</summary>
    public static StlGeometry Read(Stream stream)
    {
        // Buffer the whole stream so we can sniff ASCII vs binary and seek.
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();

        if (LooksLikeAscii(bytes))
            return ReadAscii(bytes);

        return ReadBinary(bytes);
    }

    private static bool LooksLikeAscii(byte[] bytes)
    {
        if (bytes.Length < HeaderSize + 4) return false;
        // Binary STL: header(80) + uint32 triangle count, each triangle = 50 bytes.
        var triangleCount = BitConverter.ToUInt32(bytes, HeaderSize);
        var expectedBinaryLength = HeaderSize + 4 + (long)triangleCount * 50;
        if (expectedBinaryLength == bytes.Length) return false;

        // Heuristic: starts with "solid" and contains "facet".
        var prefix = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(256, bytes.Length))
            .TrimStart();
        return prefix.StartsWith("solid", StringComparison.OrdinalIgnoreCase)
            && System.Text.Encoding.ASCII.GetString(bytes).Contains("facet", StringComparison.OrdinalIgnoreCase);
    }

    private static StlGeometry ReadBinary(byte[] bytes)
    {
        if (bytes.Length < HeaderSize + 4)
            throw new InvalidDataException("STL file is too small to be valid binary STL.");

        var triangleCount = BitConverter.ToUInt32(bytes, HeaderSize);
        var offset = HeaderSize + 4;
        const int triangleStride = 50; // normal(12) + 3 vertices(36) + attr(2)

        if (bytes.Length < offset + (long)triangleCount * triangleStride)
            throw new InvalidDataException("STL file is truncated relative to its triangle count.");

        double signedVolume = 0;
        var min = new double[] { double.MaxValue, double.MaxValue, double.MaxValue };
        var max = new double[] { double.MinValue, double.MinValue, double.MinValue };

        for (uint t = 0; t < triangleCount; t++)
        {
            var b = offset + (int)t * triangleStride + 12; // skip normal
            var v1 = ReadVec(bytes, b);
            var v2 = ReadVec(bytes, b + 12);
            var v3 = ReadVec(bytes, b + 24);

            signedVolume += SignedTetraVolume(v1, v2, v3);
            UpdateBounds(min, max, v1);
            UpdateBounds(min, max, v2);
            UpdateBounds(min, max, v3);
        }

        return Build(signedVolume, min, max, (int)triangleCount);
    }

    private static StlGeometry ReadAscii(byte[] bytes)
    {
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        var tokens = text.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        double signedVolume = 0;
        var min = new double[] { double.MaxValue, double.MaxValue, double.MaxValue };
        var max = new double[] { double.MinValue, double.MinValue, double.MinValue };
        var verts = new List<double[]>(3);
        int triangleCount = 0;

        for (int i = 0; i < tokens.Length; i++)
        {
            if (!tokens[i].Equals("vertex", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 3 >= tokens.Length) break;

            var v = new[]
            {
                ParseInvariant(tokens[i + 1]),
                ParseInvariant(tokens[i + 2]),
                ParseInvariant(tokens[i + 3]),
            };
            verts.Add(v);
            UpdateBounds(min, max, v);

            if (verts.Count == 3)
            {
                signedVolume += SignedTetraVolume(verts[0], verts[1], verts[2]);
                triangleCount++;
                verts.Clear();
            }
            i += 3;
        }

        if (triangleCount == 0)
            throw new InvalidDataException("No triangles found in ASCII STL.");

        return Build(signedVolume, min, max, triangleCount);
    }

    private static StlGeometry Build(double signedVolume, double[] min, double[] max, int triangleCount)
    {
        var volume = Math.Abs(signedVolume);
        return new StlGeometry(
            (decimal)volume,
            (decimal)(max[0] - min[0]),
            (decimal)(max[1] - min[1]),
            (decimal)(max[2] - min[2]),
            triangleCount);
    }

    private static double[] ReadVec(byte[] bytes, int at) =>
    [
        BitConverter.ToSingle(bytes, at),
        BitConverter.ToSingle(bytes, at + 4),
        BitConverter.ToSingle(bytes, at + 8),
    ];

    // Signed volume of the tetrahedron (origin, v1, v2, v3) = (v1 · (v2 × v3)) / 6.
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

    private static double ParseInvariant(string s) =>
        double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
}
