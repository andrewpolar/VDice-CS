using System;
using System.Collections.Generic;
using System.Text;

namespace VDice
{
    internal class DataHolder
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        private int _base;
        private int _size;
        private Random _rnd = new Random();
        private const int _INPUTS = 3;

        public DataHolder(int Base, int Size)
        {
            _base = Base;
            _size = Size;
        }

        private double GetOutput(double[] input)
        {
            int P = (int)Math.Round(input[0]);
            int N1 = (int)Math.Round(input[1]);
            int N2 = (int)Math.Round(input[2]);
            int S = _rnd.Next(1, _base + 1);
            double output = 0;
            if (S <= P)
            {
                for (int i = 0; i < N1; ++i)
                {
                    output += _rnd.Next(1, _base + 1);
                }
            }
            else
            {
                for (int i = 0; i < N2; ++i)
                {
                    output += _rnd.Next(1, _base + 1);
                }
            }
            return output;
        }

        public double[] GetMonteCarloSample(double[] input, int N)
        {
            double[] output = new double[N];
            for (int i = 0; i < N; ++i)
            {
                output[i] = GetOutput(input);
            }
            return output;
        }

        public double[] MakeRandomInput()
        {
            double[] input = new double[_INPUTS];
            for (int j = 0; j < _INPUTS; ++j)
            {
                input[j] = _rnd.Next(1, _base + 1);
            }
            return input;
        }

        public void MakeDataset()
        {
            for (int i = 0; i < _size; ++i)
            {
                double[] input = MakeRandomInput();
                _inputs.Add(input);
                _target.Add(GetOutput(input));
            }
        }
    }
}
