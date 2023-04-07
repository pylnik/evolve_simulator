using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sim_engine
{
    public class Species
    {
        public SpeciesParameters stat { get; set; }
        private List<float[][]> _weights;
        private List<float> _biases;
        private List<Func<float, float>> _activationFunctions;
        public Species(List<int> layers)
        {
            _weights = new List<float[][]>(layers.Count - 1);
            _biases = new List<float>(layers.Count - 1);
            _activationFunctions = new List<Func<float, float>>(layers.Count - 1);
            for (var li = 0; li < layers.Count - 1; li++)
            {
                var layer = layers[li];
                var layer_next = layers[li + 1];
                var w = new float[layer][];
                for (int i = 0; i < layer; i++)
                    w[i] = new float[layer_next];
                _weights.Add(w);
                if (li < layers.Count - 2)
                    _activationFunctions.Add(ReLU);
                else
                    _activationFunctions.Add(Sigmoid);
            }
        }

        public void Init(int seed)
        {
            var rand = new Random(seed);
            foreach (var connection in _weights)
            {
                foreach (var cw in connection)
                {
                    for (int i = 0; i < cw.Length; i++)
                    {
                        cw[i] = (float)(rand.NextDouble() * 2 - 1);
                    }
                }
            }

            for (int bi = 0; bi < _weights.Count; bi++)
            {
                _biases.Add((float)(rand.NextDouble() * 2 - 1));
            }
        }

        public float[] Think(float[] inputs)
        {
            float[] values = inputs;
            for (var layerIndex = 0; layerIndex < _weights.Count; layerIndex++)
            {
                var weight = _weights[layerIndex];
                var values_forward = new float[weight[0].Length];
                for (int i = 0; i < weight.Length; i++)
                {
                    for (int j = 0; j < weight[i].Length; j++)
                    {
                        values_forward[j] += values[i] * weight[i][j];
                    }
                }

                for (int i = 0; i < values_forward.Length; i++)
                {
                    values_forward[i] += _biases[layerIndex];
                }

                values = new float[values_forward.Length];
                for (int i = 0; i < values_forward.Length; i++)
                {
                    values[i] = _activationFunctions[layerIndex](values_forward[i]);
                }
                //values = values_forward;
            }

            //for (int i = 0; i < values.Length; i++)
            //{
            //    values[i] = Sigmoid(values[i]);
            //}
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
    }

    public class Layer
    {

    }
}
