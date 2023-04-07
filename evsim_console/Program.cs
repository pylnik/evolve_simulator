using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using sim_engine;
using ConsoleGameEngine;

namespace evsim_console
{
    class Program : ConsoleGame
    {
        static void Main(string[] args)
        {
            new Program().Construct(boardWidth, boardHeight+1, 4, 4, FramerateMode.Unlimited);
        }

        Simulator simulation;
        private static int boardWidth = 240;
        private static int boardHeight = 62;
        public override void Create()
        {
            simulation = new Simulator(
                boardWidth: boardWidth,
                boardHeight: boardHeight);
            simulation.Init(500, 30);
            //Task.Run(() => 
            //    simulation.Simulate(100));
            Engine.SetPalette(Palettes.Default);
        }

        //private List<Point> _species;
        //private List<Point> _foodPoints;
        public override void Update()
        {
            simulation.Tick();
        }

        public override void Render()
        {
            Engine.ClearBuffer();

            var _species = simulation.Population.Select(p => new Point((int)p.stat.X, (int)p.stat.Y)).ToList();
            var _foodPoints = simulation.FoodPoints.Select(p => new Point((int)p.X, (int)p.Y)).ToList();

            foreach (var foodPoint in _foodPoints)
            {
                Engine.SetPixel(foodPoint, 10);
            }

            foreach (var spec in _species)
            {
                Engine.SetPixel(spec, 1000);
            }
            Engine.DisplayBuffer();
        }
    }
}
