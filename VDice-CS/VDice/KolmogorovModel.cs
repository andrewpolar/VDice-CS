using System;
using System.Collections.Generic;
using System.Text;

namespace VDice
{
    class KolmogorovModel
    {

        //Mmodel parameters 
        int points_in_interior = 3;
        int points_in_exterior = 8;
        double muRoot = 0.1;
        double muLeaves = 0.01;
        int nEpochs = 100;
        int nLeaves = -1; //negative number means it is chosen according to theory
        //////////////////////////////////////////////////////////////////////////

        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        private double[] _xmin = null;
        private double[] _xmax = null;
        private double _targetMin;
        private double _targetMax;
        int[] _interior_structure = null;
        int[] _exterior_structure = null;

        private List<U> _ulist = new List<U>();
        private U _bigU = null;
        private Random _rnd = new Random();

        public KolmogorovModel(List<double[]> inputs, List<double> target, bool doResort = false)
        {
            _inputs = inputs;
            _target = target;

            if (inputs.Count != target.Count)
            {
                Console.WriteLine("Invalid training data");
                Environment.ExitCode = 0;
            }

            FindMinMax();
            if (doResort)
            {
                ResortRandom();
            }

            int number_of_inputs = _inputs[0].Length;
            if (nLeaves < 0)
            {
                nLeaves = number_of_inputs * 2 + 1;
            }
            _interior_structure = new int[number_of_inputs];
            for (int i = 0; i < number_of_inputs; i++)
            {
                _interior_structure[i] = points_in_interior;
            }
            _exterior_structure = new int[nLeaves];
            for (int i = 0; i < nLeaves; i++)
            {
                _exterior_structure[i] = points_in_exterior;
            }

            GenerateInitialOperators();
        }

        private void RandomSwap(List<double[]> inputs, List<double> target)
        {
            int n1 = _rnd.Next(inputs.Count);
            int n2 = _rnd.Next(inputs.Count);

            double[] tmp = new double[inputs[0].Length];
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = inputs[n1][i];
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                inputs[n1][i] = inputs[n2][i];
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                inputs[n2][i] = tmp[i];
            }
            double tmp2 = target[n1];
            target[n1] = target[n2];
            target[n2] = tmp2;
        }

        private void ResortRandom()
        {
            int count = _inputs.Count;
            for (int i = 0; i < 2 * count / 3; i++)
            {
                RandomSwap(_inputs, _target);
            }
        }

        private void FindMinMax()
        {
            int size = _inputs[0].Length;
            _xmin = new double[size];
            _xmax = new double[size];

            for (int i = 0; i < size; ++i)
            {
                _xmin[i] = double.MaxValue;
                _xmax[i] = double.MinValue;
            }

            for (int i = 0; i < _inputs.Count; ++i)
            {
                for (int j = 0; j < _inputs[i].Length; ++j)
                {
                    if (_inputs[i][j] < _xmin[j]) _xmin[j] = _inputs[i][j];
                    if (_inputs[i][j] > _xmax[j]) _xmax[j] = _inputs[i][j];
                }

            }

            _targetMin = double.MaxValue;
            _targetMax = double.MinValue;
            for (int j = 0; j < _target.Count; ++j)
            {
                if (_target[j] < _targetMin) _targetMin = _target[j];
                if (_target[j] > _targetMax) _targetMax = _target[j];
            }
        }

        public void GenerateInitialOperators()
        {
            _ulist.Clear();
            int points = _inputs[0].Length;
            for (int counter = 0; counter < nLeaves; ++counter)
            {
                U uc = new U(_xmin, _xmax, _targetMin, _targetMax, _interior_structure);
                _ulist.Add(uc);
            }

            if (null != _bigU)
            {
                _bigU.Clear();
                _bigU = null;
            }

            double[] min = new double[nLeaves];
            double[] max = new double[nLeaves];
            for (int i = 0; i < nLeaves; ++i)
            {
                min[i] = _targetMin;
                max[i] = _targetMax;
            }

            _bigU = new U(min, max, _targetMin, _targetMax, _exterior_structure);
        }

        private double[] GetVector(double[] data)
        {
            int size = _ulist.Count;
            double[] vector = new double[size];
            for (int i = 0; i < size; ++i)
            {
                vector[i] = _ulist[i].GetU(data);
            }
            return vector;
        }

        public void BuildRepresentation()
        {
            for (int step = 0; step < nEpochs; ++step)
            {
                double norm1 = 0.0;
                double norm2 = 0.0;
                double dist = 0.0;
                for (int i = 0; i < _inputs.Count; ++i)
                {
                    double[] v = GetVector(_inputs[i]);
                    double model = _bigU.GetU(v);
                    double diff = _target[i] - model;
                    double relative_diff = diff / v.Length;

                    for (int k = 0; k < _ulist.Count; ++k)
                    {
                        if (v[k] > _targetMin && v[k] < _targetMax)
                        {
                            double derrivative = _bigU.GetDerrivative(k, v[k]);
                            //if (derrivative > 0.0) derrivative = 1.0;
                            //else derrivative = -1.0;
                            _ulist[k].Update(relative_diff * derrivative, _inputs[i], muLeaves);
                        }
                    }
                    _bigU.Update(diff, v, muRoot);
                    norm1 += model * model;
                    norm2 += _target[i] * _target[i];
                    dist += diff * diff;
                }
                norm1 = Math.Sqrt(norm1);
                norm2 = Math.Sqrt(norm2);
                dist = Math.Sqrt(dist);
                dist /= ((norm1 + norm2) / 2.0);
                //Console.Write("Step {0}, dist {1:0.000}  \r", step, dist);
                //ResortRandom();
            }
            //Console.WriteLine();

            //_bigU.ShowOperatorLimits();
        }

        public double DoTest()
        {
            double RMSE = 0.0;
            int cnt = 0;
            int N = _inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                double[] data = _inputs[i];
                double[] v = GetVector(data);
                double prediction = _bigU.GetU(v);
                double diff = _target[i] - prediction;
                RMSE += diff * diff;
                ++cnt;
            }
            RMSE /= cnt;
            RMSE = Math.Sqrt(RMSE);
            //RMSE /= (_targetMax - _targetMin);

            return RMSE;
        }

        public double ComputeOutput(double[] inputs)
        {
            double[] v = GetVector(inputs);
            double output = _bigU.GetU(v);
            return output;
        }

        public void SortData()
        {
            List<double> error = new List<double>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                error.Add(_target[i] - _bigU.GetU(GetVector(_inputs[i])));
            }
            int[] indexes = new int[error.Count];
            for (int i = 0; i < indexes.Length; ++i)
            {
                indexes[i] = i;
            }
            Array.Sort(error.ToArray(), indexes);
            ResortData(indexes);
        }

        public void ResortData(int[] indexes)
        {
            int len = _inputs[0].Length;
            List<double[]> tmpInput = new List<double[]>();
            List<double> tmpTarget = new List<double>();
            foreach (int n in indexes)
            {
                double[] x = new double[len];
                for (int k = 0; k < len; ++k)
                {
                    x[k] = _inputs[n][k];
                }
                tmpInput.Add(x);
                tmpTarget.Add(_target[n]);
            }
            _inputs.Clear();
            _target.Clear();
            for (int i = 0; i < tmpInput.Count; ++i)
            {
                _inputs.Add(tmpInput[i]);
                _target.Add(tmpTarget[i]);
            }
        }

        public void ErrorTest()
        {
            List<double> error = new List<double>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                Console.WriteLine(_target[i] - _bigU.GetU(GetVector(_inputs[i])));
            }
        }

        public double ComputeAccuracy()
        {
            int N = _inputs.Count;
            double[] targetEstimate = new double[N];
            int count = 0;
            for (int i = 0; i < N; ++i)
            {
                double[] data = _inputs[i];
                double[] v = GetVector(data);
                targetEstimate[i] = _bigU.GetU(v);
                ++count;
            }
            double[] x = new double[count];
            double[] y = new double[count];
            count = 0;
            for (int i = 0; i < N; ++i)
            {
                x[count] = targetEstimate[i];
                y[count] = _target[i];
                ++count;
            }
            return Static.relativeDistance(x, y);
        }
    }
}
