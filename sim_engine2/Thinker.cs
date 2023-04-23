using System;
using System.Collections.Generic;
using System.Linq;

namespace sim_engine
{
    public class Thinker
    {
        private readonly List<int> _layers;
        private readonly int _layersCount;
        private List<Func<float, float>> _activationFunctions;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layers">number of neurons in each layer</param>
        public Thinker(List<int> layers)
        {
            _layers = layers;
            _layersCount = layers.Count;
            _activationFunctions = new List<Func<float, float>>(_layersCount);
            for (int i = 0; i < _layersCount; i++)
                _activationFunctions.Add(Tanh);
        }
        public float[] Think(float[] inputs, Genome genome)
        {
            float[] values = inputs;
            List<float[]> layersValues = new List<float[]>(_layersCount + 1);
            layersValues.Add(inputs);
            for (int i = 0; i < _layersCount; i++)
                layersValues.Add(new float[_layers[i]]);

            var l0 =
                genome.GeneList.Where(g =>
                    g.InputLayerIndex == 0 || g.OutputLayerIndex == 0).ToList();
            foreach (var gene in l0)
            {
                int outLayer = 0;
                switch (gene.InputLayerIndex)
                {
                    case 0:
                        outLayer = gene.OutputLayerIndex + 1; break;
                    case 1:
                        outLayer = gene.OutputLayerIndex + 1;
                        break;
                }
                layersValues[gene.OutputLayerIndex + 1][gene.OutputNeuronIndex] +=
                    layersValues[gene.InputLayerIndex][gene.InputNeuronIndex] * gene.WeightFloat;
            }

            for (int i = 0; i < layersValues[1].Length; i++)
            {
                layersValues[1][i] = _activationFunctions[1](layersValues[1][i]);
            }
            foreach (var gene in l0)
            {
                var geneOutputLayerIndex = gene.OutputLayerIndex + 1;
                var input = layersValues[geneOutputLayerIndex][gene.OutputNeuronIndex];
                layersValues[geneOutputLayerIndex][gene.OutputNeuronIndex] = _activationFunctions[geneOutputLayerIndex - 1](input);
            }

            var l1 =
                genome.GeneList.Where(g => g.InputLayerIndex == 1 && g.OutputLayerIndex > 0).ToList();
            foreach (var gene in l1)
            {
                layersValues[gene.InputLayerIndex + gene.OutputLayerIndex][gene.OutputNeuronIndex] +=
                    layersValues[gene.InputLayerIndex][gene.InputNeuronIndex] * gene.WeightFloat;
            }

            for (int i = 0; i < layersValues[2].Length; i++)
            {
                layersValues[2][i] = _activationFunctions[1](layersValues[2][i]);
            }

            return layersValues[2];
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
}