using System.Text.RegularExpressions;

namespace СoordinatesСonverter
{
    internal static class Program
    {
        private static void Main()
        {
            var converter = new Converter();
            
            do
            {
                Console.WriteLine("Choose conversion:");
                Console.WriteLine("1 - WGS-84 (azimuth) -> SK-42  (azimuth)");
                Console.WriteLine("2 - SK-42  (azimuth) -> WGS-84 (azimuth)");
                Console.WriteLine("3 - SK-42  (meters)  -> WGS-84 (azimuth)");
                var isConversionType = int.TryParse(Console.ReadLine(), out var conversionType);

                if (!isConversionType)
                {
                    Console.WriteLine("\nIncorrect conversion type value! Use numbers.");
                    
                    continue;
                }
                
                double latitude;
                double longitude;
                double resultLatitude;
                double resultLongitude;
                    
                switch (conversionType)
                {
                    case 1:
                    {
                        var latAndLongSplitted = GetLatitudeAndLongitude();

                        if (latAndLongSplitted is null)
                        {
                            continue;
                        }
                            
                        latitude = double.Parse(latAndLongSplitted[0]);
                        longitude = double.Parse(latAndLongSplitted[1]);
                        resultLatitude = converter.ConvertWgs84ToSk42Latitude(latitude, 0);
                        resultLongitude = converter.ConvertWgs84ToSk42Longitude(latitude, longitude, 0);

                        break;
                    }
                    case 2:
                    {
                        var latAndLongSplitted = GetLatitudeAndLongitude();

                        if (latAndLongSplitted is null)
                        {
                            continue;
                        }
                            
                        latitude = double.Parse(latAndLongSplitted[0]);
                        longitude = double.Parse(latAndLongSplitted[1]);
                        resultLatitude = converter.ConvertSk42ToWgs84Latitude(latitude, 0);
                        resultLongitude = converter.ConvertSk42ToWgs84Longitude(latitude, longitude, 0);

                        break;
                    }
                    case 3:
                    {
                        var latAndLongSplitted = GetMeters();
                        
                        break;
                    }
                    default:
                    {
                        Console.WriteLine("\nUnavailable conversion type!");
                        Console.WriteLine("Available types are:");
                        Console.WriteLine("1 - WGS-84 -> SK-42");
                        Console.WriteLine("2 - SK-42 -> WGS-84");
                            
                        continue;
                    }
                }
                    
                Console.WriteLine($"Result: {resultLatitude}, {resultLongitude}");
            } while (IsAgain());
        }

        private static bool IsAgain()
        {
            Console.WriteLine("\nQuit?");
            Console.WriteLine("1 - yes, 2 - no");
            var isSucceed = int.TryParse(Console.ReadLine(),  out var quitInt);

            if (isSucceed && quitInt is >= 1 and <= 2)
            {
                Console.WriteLine();
                return quitInt != 1;
            }
            
            Console.WriteLine("\nType appropriate value!\n");
                
            return true;
        }

        private static string[]? GetLatitudeAndLongitude()
        {
            var latLongRegex =
                new Regex(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$");
            
            Console.WriteLine("Input latitude and longitude:");
            var latAndLongString = Console.ReadLine();

            if (latAndLongString is not null && latLongRegex.IsMatch(latAndLongString))
            {
                return latAndLongString.Split(',');
            }

            Console.WriteLine("\nInvalid format or null string.");
            Console.WriteLine("Valid formats example:");
            Console.WriteLine("49.9999631298, 50.0014646551");
            Console.WriteLine("+49.9999631298, -50.0014646551");
            Console.WriteLine("49.9999631298, -50.0014646551");
            Console.WriteLine("49, 50.00001");
            Console.WriteLine("49, 50");
            Console.WriteLine("49,50");
            Console.WriteLine("49,               50");
            Console.WriteLine("49, +50");

            return null;
        }

        private static string[]? GetMeters()
        {
            return null;
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

        private readonly Ellipsoid _sk42;
        private readonly Ellipsoid _wgs84;
        
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

        public double ConvertSk42ToWgs84Meters(double x, double y, double height)
        {
            
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