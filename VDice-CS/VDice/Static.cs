using System;
using System.Collections.Generic;
using System.Text;

namespace VDice
{
    class Static
    {
       public static double relativeDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                Console.WriteLine("Static.relativeDistance wrong data passed");
                Environment.Exit(0);
            }
            double dist = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                dist += (x[i] - y[i]) * (x[i] - y[i]);
                norm1 += x[i] * x[i];
                norm2 += y[i] * y[i];
            }
            dist = Math.Sqrt(dist);
            norm1 = Math.Sqrt(norm1);
            norm2 = Math.Sqrt(norm2);
            double norm = (norm1 + norm2) / 2.0;
            return dist / norm;
        }

        static void GetCumulative(double[] v, out List<double> args, out List<double> fs)
        {
            args = new List<double>();
            fs = new List<double>();
            double prev = v[0];
            args.Add(prev);
            fs.Add(1.0);
            for (int i = 1; i < v.Length; i++)
            {
                if (v[i] != prev)
                {
                    args.Add(v[i]);
                    fs.Add(1.0);
                    prev = v[i];
                }
                else
                {
                    fs[fs.Count - 1] += 1.0;
                }
            }
            for (int i = 1; i < fs.Count; i++)
            {
                fs[i] += fs[i - 1];
            }
            double s = 0.0;
            for (int i = 0; i < fs.Count; i++)
            {
                s += fs[i];
            }
            for (int i = 0; i < fs.Count; i++)
            {
                fs[i] /= s;
            }
        }

        static double[] GetArrayOfValues(double min, double max, int N, List<double> args, List<double> fs)
        {
            double delta = (max - min) / (N - 1);
            double arg = min;
            double f = 0.0;
            List<double> values = new List<double>();
            for (int i = 0; i < N; i++)
            {
                if (arg < args[0])
                {
                    f = 0.0;
                }
                else if (arg > args[args.Count - 1])
                {
                    f = 1.0;
                }
                else
                {
                    for (int j = 1; j < args.Count; j++)
                    {
                        if (arg > args[j - 1] && arg <= args[j])
                        {
                            f = fs[j];
                            break;
                        }
                    }
                }
                values.Add(f);
                arg += delta;
            }
            return values.ToArray();
        }

        public static List<double> MedianSplit(double[] x, int depth)
        {
            List<double> medians = new List<double>();
            List<double> data = new List<double>();
            for (int k = 0; k < x.Length; ++k)
            {
                data.Add(x[k]);
            }
            Static.MedianSplit(data, depth, medians);
            medians.Sort();
            return medians;
        }

        private static void MedianSplit(List<double> x, int depth, List<double> list)
        {
            if (0 == depth) return;

            x.Sort();
            int size = x.Count;
            double median = 0;
            if (0 == size % 2)
            {
                median = (x[size / 2 - 1] + x[size / 2]) / 2.0;
            }
            else median = x[size / 2];

            list.Add(median);

            List<double> left = new List<double>();
            List<double> right = new List<double>();
            for (int i = 0; i < x.Count; i++)
            {
                if (i < size / 2)
                {
                    left.Add(x[i]);
                }
                if (0 != size % 2)
                {
                    if (i > size / 2)
                    {
                        right.Add(x[i]);
                    }
                }
                else
                {
                    if (i >= size / 2)
                    {
                        right.Add(x[i]);
                    }
                }
            }

            MedianSplit(left, depth - 1, list);
            MedianSplit(right, depth - 1, list);
        }

        public static double CVM_Statistic(List<double> x, List<double> y)
        {
            List<double> all = new List<double>();
            List<double> labels = new List<double>();
            foreach (double d in x)
            {
                all.Add(d);
                labels.Add(-1.0);
            }
            foreach (double d in y)
            {
                all.Add(d);
                labels.Add(1.0);
            }
            int[] indexes = new int[all.Count];
            for (int i = 0; i < all.Count; ++i)
            {
                indexes[i] = i;
            }
            Array.Sort(all.ToArray(), indexes);

            double UX = 0.0;
            double UY = 0.0;
            int cntx = 0;
            int cnty = 0;
            for (int i = 0; i < all.Count; ++i)
            {
                if (labels[indexes[i]] < 0)
                {
                    UX += (i - cntx) * (i - cntx);
                    ++cntx;
                }
                else
                {
                    UY += (i - cnty) * (i - cnty);
                    ++cnty;
                }
            }
            double N = x.Count;
            double M = y.Count;
            double U = UX * N + UY * M;
            double T = U / (N * M * (N + M)) - (4.0 * N * M - 1.0) / (6.0 * (M + N));
            return T;
        }

        private static List<double> GetpValues(List<double> data, int subSampleSize)
        {
            if (subSampleSize * 2 > data.Count)
            {
                System.Console.WriteLine("pValues can be estimated for subsamples smaller than {0}", data.Count / 2);
                Environment.Exit(0);
            }

            List<double> result = new List<double>();
            List<double> sample = new List<double>();
            Random random = new Random();
            for (int K = 0; K < 100; ++K)
            {
                sample.Clear();
                for (int i = 0; i < subSampleSize; ++i)
                {
                    int pos = random.Next(data.Count);
                    sample.Add(data[pos]);
                }
                double cvm = CVM_Statistic(sample, data);
                result.Add(cvm);
            }
            return result;
        }

        public static double GetProb(List<double> data, List<double> sample)
        {
            List<double> pValues = GetpValues(new List<double>(data), sample.Count);
            pValues.Sort();
            double cvm = CVM_Statistic(data, sample);
            int counter = 0;
            for (int i = pValues.Count - 1; i >= 0; --i)
            {
                if (cvm > pValues[i])
                {
                    break;
                }
                ++counter;
            }
            return counter / 100.0;
        }

        public static double GetMean(double[] x)
        {
            double mean = 0.0;
            foreach (double v in x)
            {
                mean += v;
            }
            return mean / x.Length;
        }

        public static double GetSTD(double[] x, double mean)
        {
            double std = 0.0;
            foreach (double v in x)
            {
                std += (v - mean) * (v - mean);
            }
            std /= x.Length;
            return Math.Sqrt(std);
        }
    }
}
