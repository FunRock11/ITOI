using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITOI
{
    class InterestingPoints
    {
        public int X;
        public int Y;
        public double SigmaGlobal;
        public double Sigma;
        public double Alpha;
        public int Oktava;
        public int Level;
        public int Radius;

        public InterestingPoints()
        {
            X = -1;
            Y = -1;
            SigmaGlobal = -1;
            Sigma = -1;
            Alpha = -1;
            Oktava = -1;
            Level = -1;
            Radius = -1;
        }

        public InterestingPoints(int x, int y, double sigmaglobal, double sigma, int oktava, int level)
        {
            X = x;
            Y = y;
            Sigma = sigma;
            SigmaGlobal = sigmaglobal;
            Oktava = oktava;
            Level = level;
            Alpha = -1;
            FindRadius();
        }

        private void FindRadius()
        {
            Radius = Convert.ToInt32(Math.Round(Math.Sqrt(2) * SigmaGlobal));
        }
    }
}
