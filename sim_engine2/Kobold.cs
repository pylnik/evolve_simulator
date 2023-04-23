using System;
using System.Collections.Generic;

namespace sim_engine
{
    public class Kobold
    {
        public Genome Genome { get; set; }
        public KoboldParameters Parameters { get; set; }

        public Kobold Clone(float mutationRate, Random random)
        {
            return new Kobold
            {
                Genome = Genome.Clone(mutationRate, random),
                Parameters = new KoboldParameters
                {
                    X = Parameters.X,
                    Y = Parameters.Y
                }
            };
        }
    }

    public class Gene
    {
        public float WeightNorm = 32767;
        private short _weight;
        public int InputLayerIndex { get; set; }// 1 bit
        public byte InputNeuronIndex { get; set; }// 7 bits
        public int OutputLayerIndex { get; set; }// 1 bit
        public byte OutputNeuronIndex { get; set; } // 7 bits

        public short Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                WeightFloat = Weight / WeightNorm;
            }
        } // 16 bits

        public float WeightFloat { get; private set; }
        public Gene()
        { }
        public Gene(uint gene)
        {
            // x|xxxxxxx|x|xxxxxxx|xxxxxxxxxxxxxxxx
            InputLayerIndex = (int)(gene >> 31);
            InputNeuronIndex = (byte)((gene >> 24) & 0x7F);
            OutputLayerIndex = (int)((gene >> 23) & 1);
            OutputNeuronIndex = (byte)((gene >> 16) & 0x7F);
            Weight = (short)(gene & 0xFFFF);
        }

        public uint ToNumeric()
        {
            return (uint)((InputLayerIndex << 31) | (InputNeuronIndex << 24) | (OutputLayerIndex << 23) | (OutputNeuronIndex << 16) | (ushort)Weight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mutationRate">0 .. 1</param>
        public Gene Clone(float mutationRate, Random rand)
        {
            var clone = new Gene(this.ToNumeric());
            var mut = (rand.NextDouble() - 0.5) * 2;
            if (Math.Abs(mut) < mutationRate)
                clone.Weight = (short)(this.Weight * (1 + mut));
            return clone;
        }
    }
    public class Genome
    {
        public List<Gene> GeneList { get; }

        public Genome(List<Gene> geneList)
        {
            GeneList = geneList;
        }

        public Genome Clone(float mutationRate, Random rand)
        {
            var cloneList = new List<Gene>(GeneList.Count);
            foreach (var gene in GeneList)
                cloneList.Add(gene.Clone(mutationRate, rand));

            var clone = new Genome(cloneList);
            return clone;
        }
    }

}