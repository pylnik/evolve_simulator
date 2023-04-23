using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using sim_engine;
using ConsoleGameEngine;
using sim_engine2;

namespace evsim_console
{
    class Program : ConsoleGame
    {
        static void Main(string[] args)
        {
            new Program().Construct(boardWidth, boardHeight + 1, 4, 4, FramerateMode.Unlimited);
        }

        Simulator2 simulation;
        private ISelection _selector;
        private static int boardWidth = 100;
        private static int boardHeight = 62;
        public int IterationInSimulationRun = 100;
        public int SpeciesCount = 100;
        public int HiddenNeuronsCount = 20;
        public int OutputNeuronsCount = 2;
        public int GenesCount = 100;
        public float MutationRate = 0.05f;
        public override void Create()
        {
            //_selector = new SelectionLeftX { FractionX = 0.1f };
            _selector = new SelectionCenter() { Fraction = 0.2f };

            _selector.BoardHeight = boardHeight;
            _selector.BoardWidth = boardWidth;
            simulation = new Simulator2(
                boardWidth: boardWidth,
                boardHeight: boardHeight)
            {
                MutationRate = MutationRate
            };
            simulation.SimulationFinished += Simulation_SimulationFinished;
            simulation.Init(SpeciesCount, 2, HiddenNeuronsCount, OutputNeuronsCount, GenesCount);
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    simulation.Simulate(IterationInSimulationRun);
                    simulation.Population = simulation.ProduceNextGeneration(_selector);
                    simulation.InitBoard();
                }
            });
            Engine.SetPalette(Palettes.Default);
        }

        private void Simulation_SimulationFinished()
        {
        }

        //private List<Point> _species;
        //private List<Point> _foodPoints;
        public override void Update()
        {
            //simulation.Tick();
        }

        public class CPoint
        {
            public Point Position;
            public int Color;

            public CPoint(Point position, int color)
            {
                Position = position;
                Color = color;
            }
        }
        public override void Render()
        {
            Engine.ClearBuffer();

            var _species = simulation.Population.Select(p => new CPoint(new Point((int)p.Parameters.X, (int)p.Parameters.Y), 1000)).ToList();
            //var _foodPoints = simulation.FoodPoints.Select(p => new Point((int)p.X, (int)p.Y)).ToList();

            //foreach (var foodPoint in _foodPoints)
            //{
            //    Engine.SetPixel(foodPoint, 10);
            //}

            foreach (var spec in _species)
            {
                Engine.SetPixel(spec.Position, spec.Color);
            }
            Engine.DisplayBuffer();
        }
    }
}
