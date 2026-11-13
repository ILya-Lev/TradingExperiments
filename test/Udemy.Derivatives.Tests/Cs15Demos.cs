namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class Cs15Demos(ITestOutputHelper output)
{
    public record Triangle
    {
        public double A { get; }
        public double B { get; }
        public double C { get; }

        public Triangle(double a, double b, double c)
        {
            if (a + b < c || a + c < b || b + c < a)
                throw new ArgumentException($"triangle constraint is violated with side lengths: {a}, {b}, {c}");
            if (a < 0 || b < 0 || c < 0)
                throw new ArgumentException($"side length cannot be negative, at least one is among provided: {a}, {b}, {c}");
            
            A = a;
            B = b;
            C = c;
        }
    };

    public record struct Square(double Side);

    public class Rectangle(double width, double height)
    {
        public double Width { get; } = width;
        public double Height { get; } = height;
    }

    public struct Circle(double radius)
    {
        public double Radius { get; } = radius;
    }

    union Shapes(Triangle, Square, Rectangle, Circle)
    {

        public double GetPerimeter() => this switch
        {
            Triangle t => t.A+t.B+t.C,
            Square s => 4 * s.Side,
            Rectangle r => 2 * (r.Width + r.Height),
            Circle c => 2*Math.PI * c.Radius
        };
    }

    [Fact]
    public void UnionDemo_MethodInUnion_Observe()
    {
        List<Shapes> shapes = [
            new Triangle(10,15,20),
            new Square(5),
            new Rectangle(1, Math.Sqrt(2)),
            new Circle(1/Math.PI)
        ];

        foreach (var shape in shapes)
        {
            output.WriteLine($"Shape {shape.GetType().Name} has perimeter {shape.GetPerimeter()}");
        }
    }

}
