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

        public static double[] GetKNNSample(List<double[]> inputs, List<double> targets, double[] x, double var)
        {
            List<double> distance = new List<double>();
            for (int i = 0; i < inputs.Count; i++)
            {
                double d = 0.0;
                for (int j = 0; j < inputs[i].Length; j++)
                {
                    d += (inputs[i][j] - x[j]) * (inputs[i][j] - x[j]);

                }
                distance.Add(d);
            }

            double[] dd = distance.ToArray();
            double[] tt = targets.ToArray();
            Array.Sort(dd, tt);

            List<double> result = new List<double>();
            double sumtt = 0.0;
            double sumtt2 = 0.0;
            int cnt = 0;
            double current_var = 0.0;
            while (true)
            {
                sumtt += tt[cnt];
                sumtt2 += tt[cnt] * tt[cnt];

                if (cnt > 1)
                {
                    current_var = (cnt * sumtt2 - sumtt * sumtt) / (cnt) / (cnt - 1);
                }

                result.Add(tt[cnt]);
                ++cnt;
                if (current_var > var && cnt > 3) break;
                if (cnt > 20) break;
            }

            return result.ToArray();
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

        static double GetMin(double[] v)
        {
            if (null == v) return Double.MinValue;
            double min = v[0];
            for (int i = 1; i < v.Length; i++)
            {
                if (v[i] < min) min = v[i];
            }
            return min;
        }

        static double GetMax(double[] v)
        {
            if (null == v) return Double.MaxValue;
            double max = v[0];
            for (int i = 1; i < v.Length; i++)
            {
                if (v[i] > max) max = v[i];
            }
            return max;
        }

        public static double CompareECDF(double[] x, double[] y)
        {
            Array.Sort(x);
            Array.Sort(y);

            List<double> argsx;
            List<double> fx;
            List<double> argsy;
            List<double> fy;
            GetCumulative(x, out argsx, out fx);
            GetCumulative(y, out argsy, out fy);

            double min = GetMin(x);
            double min2 = GetMin(y);
            if (min2 < min) min = min2;
            double max = GetMax(x);
            double max2 = GetMax(y);
            if (max2 > max) max = max2;
            int N = 1024;

            double[] valuesx = GetArrayOfValues(min, max, N, argsx, fx);
            double[] valuesy = GetArrayOfValues(min, max, N, argsy, fy);

            double meanDiff = 0.0;
            for (int i = 0; i < N; i++)
            {
                meanDiff += Math.Abs(valuesx[i] - valuesy[i]);
            }
            return meanDiff / N;
        }

        public static void GetExpectationAndVariance(double[] y, out double expectation, out double variance)
        {
            expectation = 0.0;
            foreach (double d in y)
            {
                expectation += d;
            }
            expectation /= (double)(y.Length);

            variance = 0.0;
            foreach (double d in y)
            {
                variance += (d - expectation) * (d - expectation);
            }
            variance /= (double)(y.Length);
        }

        public static double getMaxECDF(double[] x, double[] y)
        {
            Array.Sort(x);
            Array.Sort(y);

            List<double> argsx;
            List<double> fx;
            List<double> argsy;
            List<double> fy;
            GetCumulative(x, out argsx, out fx);
            GetCumulative(y, out argsy, out fy);

            double min = GetMin(x);
            double min2 = GetMin(y);
            if (min2 < min) min = min2;
            double max = GetMax(x);
            double max2 = GetMax(y);
            if (max2 > max) max = max2;
            int N = 1024;

            double[] valuesx = GetArrayOfValues(min, max, N, argsx, fx);
            double[] valuesy = GetArrayOfValues(min, max, N, argsy, fy);

            double maxDiff = 0.0;
            for (int i = 0; i < N; i++)
            {
                double diff = Math.Abs(valuesx[i] - valuesy[i]);
                if (diff > maxDiff) maxDiff = diff;
            }
            return maxDiff;
        }

        public static bool KSTRejected005(double[] x, double[] y)
        {
            double D = getMaxECDF(x, y);
            double D_critical = 1.358 * Math.Sqrt(1.0 / (double)(x.Length) + 1.0 / (double)(y.Length));
            if (D >= D_critical) return true;
            else return false;
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

        static List<double> SortedDistinct(List<double> x)
        {
            HashSet<double> set = new HashSet<double>();
            foreach (double d in x)
            {
                set.Add(d);
            }
            List<double> distinct = new List<double>();
            foreach (double d in set)
            {
                distinct.Add(d);
            }
            distinct.Sort();
            return distinct;
        }

        private static List<double> GetP(List<double> distinctSorted, List<double> data)
        {
            data.Sort();
            List<double> p = new List<double>();
            int distinctReference = 0;
            int currentCounter = 0;
            for (int i = 0; i < data.Count; ++i)
            {
                if (data[i] <= distinctSorted[distinctReference])
                {
                    ++currentCounter;
                }
                else
                {
                    p.Add(currentCounter);
                    currentCounter = 1;
                    ++distinctReference;
                }
            }
            if (0 != currentCounter)
            {
                p.Add(currentCounter);
            }
            for (int i = 1; i < p.Count; ++i)
            {
                p[i] += p[i - 1];
            }
            for (int i = 0; i < p.Count; ++i)
            {
                p[i] /= p[p.Count - 1];
            }
            return p;
        }

        private static List<double> GetProjected(List<double> distinctSorted, List<double> P, List<double> allDistinctSorted)
        {
            List<double> pCumulative = new List<double>();
            for (int i = 0; i < allDistinctSorted.Count; ++i)
            {
                bool isAdded = false;
                int startingIndex = 0;
                for (int k = startingIndex; k < distinctSorted.Count; ++k)
                {
                    if (distinctSorted[k] > allDistinctSorted[i])
                    {
                        if (k - 1 < 0)
                        {
                            pCumulative.Add(0.0);
                            isAdded = true;
                        }
                        else
                        {
                            pCumulative.Add(P[k - 1]);
                            isAdded = true;
                        }
                        startingIndex = k;
                        break;
                    }
                }
                if (!isAdded)
                {
                    pCumulative.Add(1.0);
                }
            }
            return pCumulative;
        }

        public static double CVM(List<double> x, List<double> y)
        {
            List<double> distinctSortedX = SortedDistinct(x);
            List<double> distinctSortedY = SortedDistinct(y);

            var all = new List<double>(x.Count + y.Count);
            all.AddRange(x);
            all.AddRange(y);

            List<double> allDistinctSorted = SortedDistinct(all);
            List<double> pX = GetP(distinctSortedX, x);
            List<double> pY = GetP(distinctSortedY, y);

            List<double> pCumulativeX = GetProjected(distinctSortedX, pX, allDistinctSorted);
            List<double> pCumulativeY = GetProjected(distinctSortedY, pY, allDistinctSorted);

            //double csv = 0.0;
            //for (int i = 0; i < pCumulativeX.Count; ++i)
            //{
            //    csv += Math.Abs(pCumulativeX[i] - pCumulativeY[i]);
            //}

            //return csv / pCumulativeX.Count;

            double csv = 0.0;
            double totalDelta = 0.0;
            for (int i = 0; i < pCumulativeX.Count - 1; ++i)
            {
                double delta = allDistinctSorted[i + 1] - allDistinctSorted[i];
                csv += Math.Abs(pCumulativeX[i] - pCumulativeY[i]) * delta;
                totalDelta += delta;
            }
            return csv / totalDelta;
        }
    }
}
