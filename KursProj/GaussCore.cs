using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursProj
{
    class GaussCore
    {
        public int Radius;
        public int Size;
        public double Sigma;
        public double[,] Matrix;

        public GaussCore() { }

        public GaussCore(double sigma)
        {
            Sigma = sigma;
            Radius = Convert.ToInt32(Math.Round(3.0 * Sigma));
            Size = 2 * Radius + 1;
            Matrix = new double[Size, Size];

            double d, stepen, e, pi, a;
            for (int hWinX = -Radius; hWinX <= Radius; hWinX++)
            {
                for (int hWinY = -Radius; hWinY <= Radius; hWinY++)
                {
                    d = Math.Sqrt(hWinX * hWinX + hWinY * hWinY);
                    stepen = -1.0 * ((d * d) / (2.0 * sigma * sigma));
                    e = Math.Pow(Math.E, stepen);
                    pi = Math.Sqrt(2.0 * Math.PI);
                    a = 1.0 / (pi * sigma);
                    Matrix[Radius + hWinY, Radius + hWinX] = a * e;
                }
            }
        }

        public GaussCore(int radius, int k)
        {
            Radius = radius;
            Sigma = (double)Radius / 3;
            Size = 2 * Radius + 1;
            Matrix = new double[Size, Size];

            double d, stepen, e, pi, a;
            for (int hWinX = -Radius; hWinX <= Radius; hWinX++)
            {
                for (int hWinY = -Radius; hWinY <= Radius; hWinY++)
                {
                    d = Math.Sqrt(hWinX * hWinX + hWinY * hWinY);
                    stepen = -1.0 * ((d * d) / (2.0 * Sigma * Sigma));
                    e = Math.Pow(Math.E, stepen);
                    pi = Math.Sqrt(2.0 * Math.PI);
                    a = 1.0 / (pi * Sigma);
                    Matrix[Radius + hWinY, Radius + hWinX] = a * e;
                }
            }
        }

    }
}
