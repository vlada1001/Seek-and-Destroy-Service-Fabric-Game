using System;

namespace Common
{
    public class Coord
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coord Scale(int factor)
        {
            X *= factor;
            Y *= factor;

            return this;
        }

        public static Coord Init()
        {
            Random rand = new();
            Coord coord = new()
            {
                X = rand.Next(-100, 100),
                Y = rand.Next(-100, 100)
            };

            return coord;
        }

        public static double Distance(Coord first, Coord second)
        {
            return Math.Sqrt(first.X * second.X + first.Y * second.Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
