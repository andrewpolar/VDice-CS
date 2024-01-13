using System;
using System.Collections.Generic;
using System.Text;

namespace VDice
{
    class SlidingKMEnsemple
    {
        private List<double[]> _inputs = null;
        private List<double> _target = null;
        private KolmogorovModel[] km = null;
        private Random _rnd = new Random();

        public SlidingKMEnsemple(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;
        }

        public void BuildModels(int SortedNBlocks, int nWantedModels)
        {
            int blockLength = _inputs.Count / SortedNBlocks;
            int shiftSize = (_inputs.Count - blockLength) / (nWantedModels - 1);
            List<int> A = new List<int>();
            List<int> B = new List<int>();
            int currentA = 0;
            int currentB = blockLength - 1;
            A.Add(currentA);
            B.Add(currentB);
            for (int i = 1; i < nWantedModels; ++i)
            {
                currentA += shiftSize;
                currentB += shiftSize;
                A.Add(currentA);
                if (_inputs.Count - 1 - currentB < shiftSize)
                {
                    currentB = _inputs.Count - 1;
                }
                B.Add(currentB);
                if (currentB >= _inputs.Count - 1)
                {
                    break;
                }
            }

            int models = A.Count;
            km = new KolmogorovModel[models];
            List<double[]> x = new List<double[]>();
            List<double> y = new List<double>();
            int nxsize = _inputs[0].Length;
            for (int i = 0; i < models; ++i)
            {
                x.Clear();
                y.Clear();
                for (int k = A[i]; k <= B[i]; ++k)
                {
                    double[] currentx = new double[nxsize];
                    for (int j = 0; j < nxsize; ++j)
                    {
                        currentx[j] = _inputs[k][j];
                    }
                    x.Add(currentx);
                    y.Add(_target[k]);
                }

                km[i] = new KolmogorovModel(x, y);
                km[i].BuildRepresentation();
                Console.WriteLine("Accuracy for estimated and acutal outputs {0:0.00}", km[i].ComputeAccuracy());
            }
            Console.WriteLine();
        }

        private double AddNoise(double value, double rate)
        {
            return (_rnd.Next(0, 100) - 50.0) / 100.0 * rate * value + value;
        }

        public double[] GetOutput(double[] x, double rate, int N)
        {
            List<double> output = new List<double>();
            for (int i = 1; i < km.Length; ++i)
            {
                output.Add(km[i].ComputeOutput(x));
            }
            for (int i = 1; i < N; ++i)
            {
                double[] z = new double[x.Length];
                for (int j = 0; j < x.Length; ++j)
                {
                    z[j] = AddNoise(x[j], rate);
                }
                for (int j = 0; j < km.Length; ++j)
                {
                    output.Add(km[j].ComputeOutput(z));
                }
            }
            return output.ToArray();
        }
    }
}
