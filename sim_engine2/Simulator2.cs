using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sim_engine;

namespace sim_engine2
{
    public class Simulator2
    {
        private readonly int _boardWidth;
        private readonly int _boardHeight;
        public List<Kobold> Population { get; set; }
        public List<Food> FoodPoints { get; private set; }
        public float MutationRate = 0.05f;

        public Simulator2(int boardWidth = 100, int boardHeight = 100)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
        }

        public int RandomSeed { get; set; } = 0;
        private Random _rand;
        private Thinker _thinker;
        public int[][] Board { get; set; }
        private int _speciesCount;
        public void Init(int speciesCount, int inputLength, int middleLength, int outputLength, int genesCount)
        {
            _speciesCount = speciesCount;
            _rand = new Random(RandomSeed);


            Population = GetPopulation(speciesCount, inputLength, middleLength, outputLength, genesCount);
            InitBoard();

            _thinker = new Thinker(new List<int> { middleLength, outputLength });
            //FoodPoints = new List<Food>();
            //for (int i = 0; i < foodPointsCount; i++)
            //{
            //    var foodNew = GetFoodPoint();
            //    FoodPoints.Add(foodNew);
            //    Debug.WriteLine($"Food added {foodNew.X}, {foodNew.Y}");
            //}
        }

        public void InitBoard()
        {
            Board = new int[_boardHeight][];
            for (int i = 0; i < _boardHeight; i++)
            {
                Board[i] = new int[_boardWidth];
                for (int j = 0; j < _boardWidth; j++)
                    Board[i][j] = -1;
            }

            for (var ki = 0; ki < Population.Count; ki++)
            {
                var kobold = Population[ki];
                Board[kobold.Parameters.Y][kobold.Parameters.X] = ki;
            }
        }

        private List<Kobold> GetPopulation(int speciesCount, int inputLength, int middleLength, int outputLength, int genesCount)
        {
            var boardPlaces = InitFreeBoardPlaces();

            var population = new List<Kobold>(speciesCount);
            for (int i = 0; i < speciesCount; i++)
            {
                var genes = GetGenome(inputLength, middleLength, outputLength, genesCount);
                var genome = new Genome(genes);
                var specie = new Kobold
                {
                    Genome = genome,
                    Parameters = GetKoboldParameters(ref boardPlaces)
                };
                population.Add(specie);
            }

            return population;
        }

        private List<int> InitFreeBoardPlaces()
        {
            var boardPlaces = new List<int>(_boardWidth * _boardHeight);
            for (int i = 0; i < _boardHeight; i++)
                for (int j = 0; j < _boardWidth; j++)
                    boardPlaces.Add(i * _boardWidth + j);
            return boardPlaces;
        }

        private KoboldParameters GetKoboldParameters(ref List<int> boardFreePlaces)
        {
            var icoord = GetNextRandomInt(0, boardFreePlaces.Count);
            var coord = boardFreePlaces[icoord];
            var kp = new KoboldParameters
            {
                X = coord % _boardWidth,
                Y = coord / _boardWidth
            };
            boardFreePlaces.Remove(icoord);
            return kp;
        }
        private List<Gene> GetGenome(int inputLength, int middleLength, int outputLength, int genesCount)
        {
            var genes = new List<Gene>(genesCount);
            var inLength = inputLength + middleLength;
            var outLength = middleLength + outputLength;

            for (int gi = 0; gi < genesCount; gi++)
            {
                var gene = GetGene(inputLength, middleLength, outputLength);
                genes.Add(gene);
            }

            return genes;
        }

        private Gene GetGene(int inputLength, int middleLength, int outputLength)
        {
            int inputLayer = GetNextRandomInt(0, 2);
            var outputLayer = GetNextRandomInt(0, 2);
            int inputIndex = 0;
            var outputIndex = 0;
            switch (inputLayer)
            {
                case 0:
                    inputIndex = GetNextRandomInt(0, inputLength);
                    break;
                case 1:
                    inputIndex = GetNextRandomInt(0, middleLength);
                    break;
            }

            switch (outputLayer)
            {
                case 0:
                    outputIndex = GetNextRandomInt(0, middleLength);
                    break;
                case 1:
                    outputIndex = GetNextRandomInt(0, outputLength);
                    break;
            }

            int weight = GetNextRandomInt(0, 0xFFFF);
            var gene = new Gene
            {
                InputLayerIndex = inputLayer,
                InputNeuronIndex = (byte)inputIndex,
                OutputLayerIndex = outputLayer,
                OutputNeuronIndex = (byte)outputIndex,
                Weight = (short)weight
            };
            return gene;
        }

        //private Food GetFoodPoint()
        //{
        //    return new Food() { X = GetRandomCoordinateX(), Y = GetRandomCoordinateY(), Nutrient = 10 };
        //}

        private int GetNextRandomInt(int min = 0, int max = 2)
        {
            return _rand.Next(min, max);
        }

        public List<Kobold> ProduceNextGeneration(ISelection selector)
        {
            var boardPlaces = InitFreeBoardPlaces();

            var survivals = selector.Select(Population);
            int productionRate = _speciesCount / survivals.Count;
            var newGen = new List<Kobold>(_speciesCount);
            foreach (var kobold in survivals)
            {
                for (int i = 0; i < productionRate; i++)
                {
                    var clone = kobold.Clone(MutationRate, _rand);
                    clone.Parameters = GetKoboldParameters(ref boardPlaces);
                    newGen.Add(clone);
                }
            }
            return newGen;
        }

        public event Action SimulationFinished;
        public void Simulate(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                Tick();
            }
            SimulationFinished?.Invoke();
        }

        public void Tick()
        {
            var outputs = new List<float[]>();
            for (var si = 0; si < Population.Count; si++)
            {
                var spec = Population[si];
                float[] inputs =
                {
                    spec.Parameters.X,
                    spec.Parameters.Y
                };
                outputs.Add(_thinker.Think(inputs, spec.Genome));
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                var output = outputs[i];
                int dx = 0, dy = 0;
                var maxindex = GetAbsMaxIndex(output);
                switch (maxindex)
                {
                    case 0:
                        dx = (int)Math.Round(output[0]);
                        break;
                    case 1:
                        dy = (int)Math.Round(output[1]);
                        break;
                    case 2:
                        var randMove = _rand.Next(0, 4);
                        dx = randMove == 0 ? 1 : randMove == 2 ? -1 : 0;
                        dy = randMove == 1 ? 1 : randMove == 3 ? -1 : 0;
                        break;
                }
                var x0 = Population[i].Parameters.X;
                var y0 = Population[i].Parameters.Y;
                var x1 = x0 + dx;
                x1 = Math.Min(_boardWidth - 1, Math.Max(0, x1));
                var y1 = y0 + dy;
                y1 = Math.Min(_boardHeight - 1, Math.Max(0, y1));
                if (Board[y1][x1] == -1)
                {
                    Board[y1][x1] = i;
                    Board[y0][x0] = -1;
                    Population[i].Parameters.X = x1;
                    Population[i].Parameters.Y = y1;
                }
            }
            //var goodDogs = new List<Species>();
            //foreach (var foodPoint in FoodPoints.ToList())
            //{
            //    var nearestSpecies = Population
            //        .OrderBy(spec => GetDistance(foodPoint.X, foodPoint.Y, spec.stat.X, spec.stat.Y))
            //        .FirstOrDefault();
            //    var dist = GetDistance(foodPoint.X, foodPoint.Y, nearestSpecies.stat.X, nearestSpecies.stat.Y);

            //    if (dist < MinDistanceToEat)
            //    {
            //        Debug.WriteLine($"Food reached {foodPoint.X}, {foodPoint.Y}");
            //        nearestSpecies.stat.Energy = MaxEnergy;
            //        nearestSpecies.stat.Life =
            //            Math.Max(MaxEnergy, nearestSpecies.stat.Life + foodPoint.Nutrient);
            //        nearestSpecies.stat.Fertilation += 1;
            //        goodDogs.Add(nearestSpecies);
            //        FoodPoints.Remove(foodPoint);
            //        var foodNew = GetFoodPoint();
            //        FoodPoints.Add(foodNew);
            //        Debug.WriteLine($"Food added {foodNew.X}, {foodNew.Y}");
            //    }
            //}

            //foreach (var spec in Population.ToList())
            //{
            //    if (spec.stat.Life <= 0)
            //    {
            //        Population.Remove(spec);
            //    }
            //}

            //CloneIfPossible(goodDogs);
            ////Debug.WriteLine($"Species: {Population.Count}, food points {FoodPoints.Count}");
        }

        //private void CloneIfPossible(List<Species> goodDogs)
        //{
        //    foreach (var goodDog in goodDogs)
        //    {
        //        if (goodDog.stat.Fertilation < FertilationThreshold)
        //            continue;
        //        var clones = goodDog.Clone(1, mutationRate: MutationRate);
        //        goodDog.stat.Fertilation = 0;
        //        goodDog.stat.Life = MaxLife;
        //        goodDog.stat.Energy = MaxEnergy;
        //        foreach (var clone in clones)
        //        {
        //            var speciesParameters = GetParameters();
        //            clone.stat.Direction = speciesParameters.Direction;
        //            clone.stat.Energy = speciesParameters.Energy;
        //            clone.stat.Life = speciesParameters.Life;
        //        }

        //        Population.AddRange(clones);
        //    }
        //}

        //private static float GetDistance(float x1, float y1, float x2, float y2)
        //{
        //    return MathNet.Numerics.Distance.Euclidean(new[] { x1, y1 }, new[] { x2, y2 });
        //}
        private int GetAbsMaxIndex(float[] array)
        {
            int maxAbsIndex = 0;

            for (int i = 1; i < array.Length; i++)
                if (Math.Abs(array[i]) > Math.Abs(array[maxAbsIndex]))
                    maxAbsIndex = i;

            return maxAbsIndex;
        }
    }
}
