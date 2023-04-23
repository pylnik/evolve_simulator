using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sim_engine;

namespace sim_engine2
{
    public interface ISelection
    {
        int BoardWidth { get; set; }
        int BoardHeight { get; set; }
        List<Kobold> Select(List<Kobold> population);
    }

    public class SelectionLeftX : ISelection
    {
        public float FractionX = 0.1f;
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }

        public List<Kobold> Select(List<Kobold> population)
        {
            return population.Where(k => k.Parameters.X < BoardWidth * FractionX).ToList();// || k.Parameters.X > boardWidth * (1 - fractionX)).ToList();

        }
    }
    public class SelectionLeftRightX : ISelection
    {
        public float FractionX = 0.1f;
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }

        public List<Kobold> Select(List<Kobold> population)
        {
            return population.Where(k => k.Parameters.X < BoardWidth * FractionX || k.Parameters.X > BoardWidth * (1 - FractionX)).ToList();

        }
    }

    public class SelectionCorner : ISelection
    {
        public float Fraction = 0.1f;
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }

        public List<Kobold> Select(List<Kobold> population)
        {
            return population.Where(k =>
                k.Parameters.X < BoardWidth * Fraction && k.Parameters.Y < BoardHeight * Fraction
                || k.Parameters.X > BoardWidth * (1 - Fraction) && k.Parameters.Y > BoardHeight * (1 - Fraction)
                || k.Parameters.X < BoardWidth * Fraction && k.Parameters.Y > BoardHeight * (1 - Fraction)
                || k.Parameters.X > BoardWidth * (1 - Fraction) && k.Parameters.Y < BoardHeight * Fraction
                ).ToList();

        }
    }

    public class SelectionCenter : ISelection
    {
        public float Fraction = 0.1f;
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }

        public List<Kobold> Select(List<Kobold> population)
        {
            int centerX = BoardWidth / 2;
            int centerY = BoardHeight / 2;
            return population.Where(k =>
                k.Parameters.X > centerX * (1 - Fraction) && k.Parameters.Y > centerY * (1 - Fraction)
                || k.Parameters.X < centerX * (1 + Fraction) && k.Parameters.Y < centerY * (1 + Fraction)
                || k.Parameters.X > centerX * (1 - Fraction) && k.Parameters.Y < centerY * (1 + Fraction)
                || k.Parameters.X < centerX * (1 + Fraction) && k.Parameters.Y > centerY * (1 - Fraction)
            ).ToList();

        }
    }

}
