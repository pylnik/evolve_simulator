using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace sim_engine
{
    public class Simulator
    {
        private readonly int _boardWidth;
        private readonly int _boardHeight;
        public List<Species> Population { get; private set; }
        public List<Food> FoodPoints { get; private set; }
        public List<int> Layers { get; set; }

        public Simulator(int boardWidth = 100, int boardHeight = 100)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
        }

        private void InitLayers()
        {
            Layers = new List<int>(4)
            {
                2,// angle and distance to the closest food point
                50,
                50,
                2// rotation increment, direction (positive -> forward, negative -> backward)
            };
        }
        public void Init(int speciesCount, int foodPointsCount)
        {
            InitLayers();
            Population = new List<Species>(speciesCount);
            for (int i = 0; i < speciesCount; i++)
            {
                var specie = new Species(Layers);
                specie.Init(i);
                specie.stat = GetParameters();
                Population.Add(specie);
            }

            FoodPoints = new List<Food>();
            for (int i = 0; i < foodPointsCount; i++)
            {
                var foodNew = GetFoodPoint();
                FoodPoints.Add(foodNew);
                Debug.WriteLine($"Food added {foodNew.X}, {foodNew.Y}");
            }
        }

        private Food GetFoodPoint()
        {
            return new Food() { X = GetRandomCoordinateX(), Y = GetRandomCoordinateY(), Nutrient = 10 };
        }

        private Random _rnd = new Random(0);
        private SpeciesParameters GetParameters()
        {
            var specParams = new SpeciesParameters()
            {
                Direction = (float)(GetNextRandom() * Math.PI * 2),
                X = GetRandomCoordinateX(),
                Y = GetRandomCoordinateY(),
                Energy = 100,
                Life = 100
            };
            return specParams;
        }

        private float GetRandomCoordinateX()
        {
            return GetNextRandom() * _boardWidth;
        }

        private float GetNextRandom()
        {
            return (float)_rnd.NextDouble();
        }
        
        private float GetRandomCoordinateY()
        {
            return GetNextRandom() * _boardHeight;
        }

        private const float MaxAngleIncrement = (float)(Math.PI / 20);//+-30 degree
        private const float MaxMovement = 10;//points to move
        private const float MinDistanceToEat = 1;
        private const float MaxEnergy = 1000;
        public void Simulate(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                Tick();
            }
        }

        public void Tick()
        {
            if (Population.Count == 0)
                return;
            foreach (var spec in Population)
            {
                var foodPoint = FoodPoints
                    .OrderBy(fp => GetDistance(fp.X, fp.Y, spec.stat.X, spec.stat.Y))
                    .FirstOrDefault();
                if (foodPoint != null)
                {
                    var angleRad = Math.Atan2(foodPoint.Y - spec.stat.Y, foodPoint.X - spec.stat.X);
                    var angleNorm = (float)((angleRad + Math.PI) / (Math.PI * 2));
                    var dist = GetDistance(foodPoint.X, foodPoint.Y, spec.stat.X, spec.stat.Y);
                    var thoughts = spec.Think(new[] { angleNorm, dist });
                    if (thoughts != null)
                    {
                        var angleN = (thoughts[0] - 0.5f) * 2;
                        var movementN = (thoughts[1] - 0.5f) * 2; // norm to -1:1
                        var angleR = angleN * MaxAngleIncrement * 2;
                        spec.stat.Direction += angleR;
                        var sin = Math.Sin(spec.stat.Direction);
                        var cos = Math.Cos(spec.stat.Direction);
                        var hypothen = Math.Sign(movementN) * Math.Min(Math.Abs(movementN), MaxMovement);
                        var dy = (float)(sin * hypothen);
                        var dx = (float)(cos * hypothen);
                        var newX = spec.stat.X + dx;
                        newX = Math.Min(_boardWidth, Math.Max(0, newX));
                        var newY = spec.stat.Y + dy;
                        newY = Math.Min(_boardHeight, Math.Max(0, newY));
                        var energyConsumption =
                            Distance.Euclidean(new[] { spec.stat.X, spec.stat.Y },
                                new[] { newX, newY });
                        spec.stat.X = newX;
                        spec.stat.Y = newY;

                        spec.stat.Energy -= energyConsumption;
                    }
                }

                spec.stat.Life--;
            }

            foreach (var foodPoint in FoodPoints.ToList())
            {
                var nearestSpecies = Population
                    .OrderBy(spec => GetDistance(foodPoint.X, foodPoint.Y, spec.stat.X, spec.stat.Y))
                    .FirstOrDefault();
                var dist = GetDistance(foodPoint.X, foodPoint.Y, nearestSpecies.stat.X, nearestSpecies.stat.Y);

                if (dist < MinDistanceToEat)
                {
                    FoodPoints.Remove(foodPoint);
                    Debug.WriteLine($"Food reached {foodPoint.X}, {foodPoint.Y}");
                    nearestSpecies.stat.Energy = MaxEnergy;
                    nearestSpecies.stat.Life =
                        Math.Max(MaxEnergy, nearestSpecies.stat.Life + foodPoint.Nutrient);
                    var foodNew = GetFoodPoint();
                    FoodPoints.Add(foodNew);
                    Debug.WriteLine($"Food added {foodNew.X}, {foodNew.Y}");
                }
            }

            foreach (var spec in Population.ToList())
            {
                if (spec.stat.Life <= 0)
                {
                    Population.Remove(spec);
                }
            }
            //Debug.WriteLine($"Species: {Population.Count}, food points {FoodPoints.Count}");
        }

        private static float GetDistance(float x1, float y1, float x2, float y2)
        {
            return MathNet.Numerics.Distance.Euclidean(new[] { x1, y1 }, new[] { x2, y2 });
        }
    }
}
