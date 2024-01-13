using System;
using System.Collections.Generic;
using System.Text;

namespace VDice
{
    class U
    {
        private List<PLL> _plist = new List<PLL>();

        public U(double[] xmin, double[] xmax, double targetMin, double targetMax, int[] layers)
        {
            double ymin = targetMin / layers.Length;
            double ymax = targetMax / layers.Length;
            for (int i = 0; i < layers.Length; ++i)
            {
                PLL pll = new PLL(xmin[i], xmax[i], ymin, ymax, layers[i]);
                _plist.Add(pll);
            }
            SetRandom(ymin, ymax);
        }

        public void Clear()
        {
            _plist.Clear();
        }

        public double GetDerrivative(int layer, double x)
        {
            return _plist[layer].GetDerrivative(x);
        }

        public void SetRandom(double ymin, double ymax)
        {
            foreach (PLL pll in _plist)
            {
                pll.SetRandom(ymin / _plist.Count, ymax / _plist.Count);
            }
        }

        public void Update(double delta, double[] inputs, double mu)
        {
            int i = 0;
            foreach (PLL pll in _plist)
            {
                pll.Update(inputs[i++], delta / _plist.Count, mu);
            }
        }

        public double GetU(double[] inputs)
        {
            double f = 0.0;
            int i = 0;
            foreach (PLL pll in _plist)
            {
                f += pll.GetFunctionValue(inputs[i++]);
            }
            return f;
        }

        public void ShowOperatorLimits()
        {
            Console.WriteLine("----- Top operator limits -----");
            foreach (PLL pll in _plist)
            {
                String S = String.Format("(min, max) = ({0:0.00}, {1:0.00})", pll.GetXmin(), pll.GetXmax());
                Console.WriteLine(S);

            }
            Console.WriteLine("-------------------------------");
        }
    }
}
