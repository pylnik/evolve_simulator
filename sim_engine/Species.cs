using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearRegression;

namespace sim_engine
{
    public class Species
    {
        public SpeciesParameters stat { get; set; }
        public List<float[][]> Weights { get; set; }
        public List<float[]> Biases { get; set; }
        private List<Func<float, float>> _activationFunctions;
        private readonly List<int> _layersConfig;
        public Species(List<int> layers)
        {
            _layersConfig = layers;
            Weights = new List<float[][]>(layers.Count - 1);
            Biases = new List<float[]>(layers.Count - 1);
            _activationFunctions = new List<Func<float, float>>(layers.Count - 1);
            for (var li = 0; li < layers.Count - 1; li++)
            {
                var layer = layers[li];
                var layer_next = layers[li + 1];
                var w = new float[layer][];
                for (int i = 0; i < layer; i++)
                    w[i] = new float[layer_next];
                Weights.Add(w);
                Biases.Add(new float[layer_next]);
                _activationFunctions.Add(Tanh);
                //if (li < layers.Count - 2)
                //    _activationFunctions.Add(ReLU);
                //else
                //    _activationFunctions.Add(Sigmoid);
            }
        }

        public void Init(int seed)
        {
            var rand = new Random(seed);
            foreach (var connection in Weights)
                foreach (var cw in connection)
                    for (int i = 0; i < cw.Length; i++)
                        cw[i] = (float)(rand.NextDouble() * 2 - 1);

            foreach (var b in Biases)
                for (var i = 0; i < b.Length; i++)
                    b[i] = (float)(rand.NextDouble() * 2 - 1);
        }

        public List<Species> Clone(int numberOfClones = 1, float mutationRate = 0.01f)
        {
            var clones = new List<Species>(numberOfClones);
            var random = new Random();
            for (int ci = 0; ci < numberOfClones; ci++)
            {
                var w1 = new List<float[][]>(Weights.Count);
                foreach (var w0 in Weights)
                {
                    var ww = new float[w0.Length][];
                    w1.Add(ww);
                    for (int j = 0; j < w0.Length; j++)
                    {
                        ww[j] = new float[w0[j].Length];
                        w0[j].CopyTo(ww[j], 0);
                        for (int k = 0; k < ww[j].Length; k++)
                            if (random.NextDouble() < mutationRate)
                            {
                                var wr = (float)(random.NextDouble() - 0.5) * 2 * mutationRate;
                                ww[j][k] += wr;
                            }
                    }
                }

                var b1 = Biases.ToList();
                foreach (var b in b1)
                    for (int j = 0; j < b.Length; j++)
                        if (random.NextDouble() < mutationRate)
                        {
                            var wr = (float)(random.NextDouble() - 0.5) * 2 * mutationRate;
                            b[j] += wr;
                        }

                var sp = new Species(_layersConfig)
                {
                    Weights = w1,
                    Biases = b1,
                    stat = new SpeciesParameters()
                    {
                        Direction = stat.Direction,
                        X = stat.X,
                        Y = stat.Y,
                        Life = stat.Life,
                        Energy = stat.Energy
                    }
                };
                clones.Add(sp);
            }

            return clones;
        }

        public float[] Think(float[] inputs)
        {
            float[] values = inputs;
            for (var layerIndex = 0; layerIndex < Weights.Count; layerIndex++)
            {
                var weight = Weights[layerIndex];
                var biases = Biases[layerIndex];
                var values_forward = new float[weight[0].Length];
                for (int i = 0; i < weight.Length; i++)
                    for (int j = 0; j < weight[i].Length; j++)
                        values_forward[j] += values[i] * weight[i][j];

                for (int i = 0; i < values_forward.Length; i++)
                    values_forward[i] += biases[i];

                values = new float[values_forward.Length];
                for (int i = 0; i < values_forward.Length; i++)
                    values[i] = _activationFunctions[layerIndex](values_forward[i]);
            }
            return values;
        }


        private float ReLU(float val)
        {
            if (float.IsNaN(val))
                val = 10;
            return Math.Max(val, 0);
        }

        private float Sigmoid(float val)
        {
            return (float)(1 / (1 + Math.Exp(-val)));
        }

        private float Tanh(float val)
        {
            return (float)Math.Tanh(val);
        }
    }

    public class Layer
    {

    }
}
