namespace СoordinatesСonverter
{
    internal class Program
    {
        private static void Main()
        {
            var _converter = new Converter();
            var a = _converter.ConvertWgs84ToSk42Latitude(50, 0);

            Console.WriteLine(a);
        }
    }

    public class Converter
    {
        // Transformation elements
        const double dx = 28;
        const double dy = -130;
        const double dz = -95;
        const double wx = 0;
        const double wy = 0;
        const double wz = 0;
        const double ms = 0;

        private Ellipsoid _sk42;
        private Ellipsoid _wgs84;
        
        public Converter()
        {
            _sk42 = new Ellipsoid(
                Helpers.KrasovskySemiMajorAxis,
                Helpers.KrasovskyFlattening,
                Helpers.KrasovskyEccentricitySquare);

            _wgs84 = new Ellipsoid(
                Helpers.Wgs84SemiMajorAxis,
                Helpers.Wgs84SemiFlattening,
                Helpers.Wgs84EccentricitySquare);
        }

        public double ConvertWgs84ToSk42Latitude(double latitude, double height)
        {
            return latitude - ChangeLatitude(latitude, height) / 3600;
        }

        public double ConvertSk42ToWgs84Latitude(double latitude, double height)
        {
            return latitude + ChangeLatitude(latitude, height) / 3600;
        }
        
        public double ConvertWgs84ToSk42Longitude(double latitude, double longitude, double height)
        {
            return longitude - ChangeLongitude(latitude, longitude, height) / 3600;
        }

        public double ConvertSk42ToWgs84Longitude(double latitude, double longitude, double height)
        {
            return longitude + ChangeLongitude(latitude, longitude, height) / 3600;
        }

        private double ChangeLatitude(double latitude, double height)
        {
            var da = _wgs84.SemiMajorAxis - _sk42.SemiMajorAxis;
            var avrgSemiMajorAxis = AverageSemiMajorAxis(_sk42.SemiMajorAxis, _wgs84.SemiMajorAxis);
            var avrgEccentricitySquare = AverageEccentricitySquare(
                _sk42.EccentricitySquare,
                _wgs84.EccentricitySquare);
            var b = latitude * Helpers.Pi / (double)180;
            var m = avrgSemiMajorAxis * (1 - avrgEccentricitySquare) / Math.Pow((1 - avrgEccentricitySquare * Math.Pow(Math.Sin(b), 2)), 1.5);
            var n = avrgSemiMajorAxis * Math.Pow((1 - avrgEccentricitySquare * Math.Pow(Math.Sin(b), 2)), -0.5);
            
            return Helpers.Ro / (m + height) * (n / avrgSemiMajorAxis * avrgEccentricitySquare * Math.Sin(b) * Math.Cos(b) * da);
        }

        private double ChangeLongitude(double latitude, double longitude, double height)
        {
            var avrgSemiMajorAxis = AverageSemiMajorAxis(_sk42.SemiMajorAxis, _wgs84.SemiMajorAxis);
            var avrgEccentricitySquare = AverageEccentricitySquare(
                _sk42.EccentricitySquare,
                _wgs84.EccentricitySquare);
            var b = latitude * Helpers.Pi / (double)180;
            var l = longitude * Helpers.Pi / (double)180;
            var n = avrgSemiMajorAxis * Math.Pow((1 - avrgEccentricitySquare * Math.Pow(Math.Sin(b), 2)), -0.5);
            
            return Helpers.Ro / ((n + height) * Math.Cos((b)) * (-dx * Math.Sin((l) + dy * Math.Cos(l))));
        }
        
        private static double AverageSemiMajorAxis(double semiMajorAxis1, double semiMajorAxis2) 
            => (semiMajorAxis1 + semiMajorAxis2) / 2;

        private static double AverageEccentricitySquare(double eccentricitySquare1, double eccentricitySquare2) 
            => (eccentricitySquare1 + eccentricitySquare2) / 2;
    }

    public struct Ellipsoid
    {
        public double SemiMajorAxis { get; private set; }
        public double Flattening { get; private set; }
        public double EccentricitySquare { get; private set; }

        public Ellipsoid(double semiMajorAxis, double flattening, double eccentricitySquare)
        {
            SemiMajorAxis = semiMajorAxis;
            Flattening = flattening;
            EccentricitySquare = eccentricitySquare;
        }
    }

    public static class Helpers
    {
        public const double Pi = 3.14159265358979;
        public const double Ro = 206264.8062;
        public const double KrasovskySemiMajorAxis = 6378245;
        public const double KrasovskyFlattening = 1 / 298.3;
        public const double KrasovskyEccentricitySquare = 2 * KrasovskyFlattening - KrasovskyFlattening * KrasovskyFlattening;
        public const double Wgs84SemiMajorAxis = 6378137;
        public const double Wgs84SemiFlattening = 1 / 298.257223563;
        public const double Wgs84EccentricitySquare = 2 * Wgs84SemiFlattening - Wgs84SemiFlattening * Wgs84SemiFlattening;
    }
}