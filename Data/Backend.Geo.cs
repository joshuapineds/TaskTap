namespace TaskTap.Data;

public static partial class Backend
{
    /// <summary>Cached barangay centres keyed by barangay number ("176").</summary>
    public static Dictionary<string, (double Lat, double Lng)> LoadBarangayCoordinates()
    {
        var result = new Dictionary<string, (double, double)>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, lat, lng) in Db.Query(
                     "SELECT name, latitude, longitude FROM barangays WHERE latitude IS NOT NULL AND longitude IS NOT NULL",
                     r => (Name: r.Str("name"), Lat: Convert.ToDouble(r.GetValue(r.GetOrdinal("latitude"))), Lng: Convert.ToDouble(r.GetValue(r.GetOrdinal("longitude"))))))
        {
            result[BarangayNumber(name)] = (lat, lng);
        }
        return result;
    }

    public static void SaveBarangayCoordinates(string number, double lat, double lng) =>
        Db.NonQuery(
            "UPDATE barangays SET latitude = @a, longitude = @b WHERE TRIM(REPLACE(REPLACE(name, 'Brgy.', ''), 'Barangay', '')) = @n",
            ("@a", lat), ("@b", lng), ("@n", number));
}
