using System;
using System.Collections.Generic;

namespace FlightSim
{
    public enum FacilityType
    {
        Airport,
        Waypoint,
        Vor,
        Ndb,
    }

    public sealed class FacilityIndex
    {
        private readonly Dictionary<FacilityType, List<Facility>> _byType = new()
        {
            [FacilityType.Airport] = new List<Facility>(),
            [FacilityType.Waypoint] = new List<Facility>(),
            [FacilityType.Vor] = new List<Facility>(),
            [FacilityType.Ndb] = new List<Facility>(),
        };

        public IReadOnlyList<Facility> GetFacilities(FacilityType type) => _byType[type];

        public void Clear(FacilityType type) => _byType[type].Clear();

        public void Add(FacilityType type, string ident, string region, double latDeg, double lonDeg)
        {
            _byType[type].Add(new Facility(type, ident, region, latDeg, lonDeg));
        }

        public string? FindNearestIdent(FacilityType type, double latDeg, double lonDeg, double maxNm = 5.0)
        {
            Facility? best = null;
            double bestNm = double.MaxValue;

            var list = _byType[type];
            for (int i = 0; i < list.Count; i++)
            {
                var f = list[i];
                var dNm = HaversineNm(latDeg, lonDeg, f.LatitudeDeg, f.LongitudeDeg);
                if (dNm < bestNm)
                {
                    bestNm = dNm;
                    best = f;
                }
            }

            if (best is null || bestNm > maxNm) return null;
            return best.Ident;
        }

        // Convenience wrappers (optional)
        public string? FindNearestAirportIdent(double latDeg, double lonDeg, double maxNm = 5.0)
            => FindNearestIdent(FacilityType.Airport, latDeg, lonDeg, maxNm);

        public string? FindNearestWaypointIdent(double latDeg, double lonDeg, double maxNm = 5.0)
            => FindNearestIdent(FacilityType.Waypoint, latDeg, lonDeg, maxNm);

        public string? FindNearestVorIdent(double latDeg, double lonDeg, double maxNm = 10.0)
            => FindNearestIdent(FacilityType.Vor, latDeg, lonDeg, maxNm);

        public string? FindNearestNdbIdent(double latDeg, double lonDeg, double maxNm = 10.0)
            => FindNearestIdent(FacilityType.Ndb, latDeg, lonDeg, maxNm);

        private static double HaversineNm(double lat1, double lon1, double lat2, double lon2)
        {
            const double Rm = 6371000.0; // meters
            static double ToRad(double deg) => deg * Math.PI / 180.0;

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var meters = Rm * c;
            return meters / 1852.0; // NM
        }
    }

    public record Facility(
        FacilityType Type,
        string Ident,
        string Region,
        double LatitudeDeg,
        double LongitudeDeg);
}
