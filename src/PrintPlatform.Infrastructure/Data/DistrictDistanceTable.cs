using PrintPlatform.Application.Marketplace;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// Hardcoded approximate driving distances (km) between Greater Cairo districts
/// (Cairo + Giza). Avoids a PostGIS dependency for the MVP.
/// <para>
/// Lookups are case-insensitive and symmetric. Same-district distance is 0.
/// Unknown pairs fall back to <see cref="DefaultDistanceKm"/>.
/// </para>
/// </summary>
public sealed class DistrictDistanceTable : IDistrictDistanceCalculator
{
    /// <summary>Fallback distance used when a district pair is not in the table.</summary>
    public const double DefaultDistanceKm = 25d;

    // Symmetric pair table keyed by an order-independent composite key.
    private static readonly IReadOnlyDictionary<string, double> Distances = BuildTable();

    public double GetDistanceKm(string fromDistrict, string toDistrict)
    {
        if (string.IsNullOrWhiteSpace(fromDistrict) || string.IsNullOrWhiteSpace(toDistrict))
            return DefaultDistanceKm;

        var a = Normalize(fromDistrict);
        var b = Normalize(toDistrict);
        if (a == b) return 0d;

        return Distances.TryGetValue(Key(a, b), out var km) ? km : DefaultDistanceKm;
    }

    private static string Normalize(string s) => s.Trim().ToLowerInvariant();

    private static string Key(string a, string b)
        => string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";

    private static Dictionary<string, double> BuildTable()
    {
        // (districtA, districtB, km) — populate symmetrically below.
        (string A, string B, double Km)[] pairs =
        [
            ("Maadi",          "Nasr City",       12),
            ("Maadi",          "Heliopolis",      18),
            ("Maadi",          "Dokki",           11),
            ("Maadi",          "Mohandessin",     13),
            ("Maadi",          "6th of October",  28),
            ("Maadi",          "Downtown",        9),
            ("Maadi",          "New Cairo",       15),
            ("Maadi",          "Zamalek",         10),
            ("Maadi",          "Giza",            12),
            ("Nasr City",      "Heliopolis",      6),
            ("Nasr City",      "Dokki",           14),
            ("Nasr City",      "Mohandessin",     15),
            ("Nasr City",      "New Cairo",       10),
            ("Nasr City",      "Downtown",        9),
            ("Nasr City",      "6th of October",  32),
            ("Heliopolis",     "New Cairo",       14),
            ("Heliopolis",     "Downtown",        11),
            ("Heliopolis",     "Mohandessin",     16),
            ("Heliopolis",     "Dokki",           15),
            ("Dokki",          "Mohandessin",     3),
            ("Dokki",          "Zamalek",         4),
            ("Dokki",          "Giza",            6),
            ("Dokki",          "Downtown",        5),
            ("Dokki",          "6th of October",  22),
            ("Mohandessin",    "Zamalek",         5),
            ("Mohandessin",    "Giza",            7),
            ("Mohandessin",    "6th of October",  21),
            ("Mohandessin",    "Downtown",        6),
            ("Zamalek",        "Downtown",        3),
            ("Zamalek",        "Giza",            9),
            ("Giza",           "6th of October",  18),
            ("Giza",           "Downtown",        8),
            ("New Cairo",      "6th of October",  42),
            ("New Cairo",      "Downtown",        18),
            ("6th of October", "Downtown",        24),
        ];

        var table = new Dictionary<string, double>(pairs.Length, StringComparer.Ordinal);
        foreach (var (a, b, km) in pairs)
            table[Key(Normalize(a), Normalize(b))] = km;
        return table;
    }
}
