namespace RectangleTrainer.Core.Models;

public class Rectangle
{
    public double A { get; init; }
    public double B { get; init; }
    public double X1 { get; init; }
    public double Y1 { get; init; }
    public double X2 { get; init; }
    public double Y2 { get; init; }

    public double Perimeter => 2 * (A + B);
    public double Area => A * B;

    public static Rectangle FromSides(double a, double b) => new() { A = a, B = b };
    public static Rectangle FromPoints(double x1, double y1, double x2, double y2)
    {
        double a = Math.Abs(x2 - x1);
        double b = Math.Abs(y2 - y1);
        return new()
        {
            A = a,
            B = b,
            X1 = Math.Min(x1, x2),
            Y1 = Math.Min(y1, y2),
            X2 = Math.Max(x1, x2),
            Y2 = Math.Max(y1, y2)
        };
    }
}