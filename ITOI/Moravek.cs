using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITOI
{
    class Moravek
    {
        public Img Image;
        public Img ImageWithPoints;
        public Img ImageWithANMS;
        public int WindowRadius;
        public double[,] S;
        public bool[,] InterestingPoints;
        public double R; // Отбираются точки, которые больше R * max
        public int NPoints;
        public int NeedPoints;

        private double MAXmin = -999999999;
        private double MINmin = 999999999;

        public Moravek(Img image, int windowradius, double r)
        {
            Image = image;
            WindowRadius = windowradius;
            R = r;

            MoravekS();
            IntPoints1();
            IWP(1);
        }

        private void MoravekS()
        {
            int k1 = WindowRadius + 1;
            S = new double[Image.Height, Image.Width];

            for (int y = k1; y < Image.Height - k1; y++)
            {
                for (int x = k1; x < Image.Width - k1; x++)
                {
                    double Q = 999999999;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if  (dx == 0 && dy == 0)
                            {
                                continue;
                            }
                            else
                            {
                                double C = 0;
                                for (int hWinX = -WindowRadius; hWinX <= WindowRadius; hWinX++)
                                {
                                    for (int hWinY = -WindowRadius; hWinY <= WindowRadius; hWinY++)
                                    {
                                        C += Math.Pow((double)Image.GrayMatrix[y + hWinY, x + hWinX] - (double)Image.GrayMatrix[y + hWinY + dx, x + hWinX + dy], 2);
                                    }
                                }
                                if (C < Q)
                                {
                                    Q = C;
                                }
                            }
                        }
                    }
                    S[y, x] = Q;

                    if (S[y, x] > MAXmin)
                    {
                        MAXmin = S[y, x];
                    }
                    if (S[y, x] < MINmin)
                    {
                        MINmin = S[y, x];
                    }
                }
            }
            for (int y = 0; y < k1; y++)
            {
                for (int x = 0; x < k1; x++)
                {
                    S[y, x] = MINmin;
                    S[y, x] = MINmin;
                }
            }
            for (int y = Image.Height - k1; y < Image.Height; y++)
            {
                for (int x = Image.Width - k1; x < Image.Width; x++)
                {
                    S[y, x] = MINmin;
                    S[y, x] = MINmin;
                }
            }
        }

        private void IntPoints1()
        {
            InterestingPoints = new bool[Image.Height, Image.Width];
            double T = (MAXmin - MINmin) * R;

            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    if (S[y, x] > T)
                    {
                        InterestingPoints[y, x] = true;
                    }
                    else
                    {
                        InterestingPoints[y, x] = false;
                    }
                }
            }

            
            for (int y = WindowRadius; y < Image.Height - WindowRadius; y++)
            {
                for (int x = WindowRadius; x < Image.Width - WindowRadius; x++)
                {
                    if (InterestingPoints[y, x])
                    {
                        for (int hWinX = -WindowRadius; hWinX <= WindowRadius; hWinX++)
                        {
                            for (int hWinY = -WindowRadius; hWinY <= WindowRadius; hWinY++)
                            {
                                if (x + hWinX < Image.Width && x + hWinX >= 0
                                    && y + hWinY < Image.Height && y + hWinY >= 0)
                                {
                                    if (hWinX == 0 && hWinY == 0)
                                    {
                                        continue;
                                    }
                                    else if (S[y + hWinY, x + hWinX] < S[y, x])
                                    {
                                        InterestingPoints[y + hWinY, x + hWinX] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
        }

        private void IWP(int r)
        {
            ImageWithPoints = new Img(Image.GrayMatrix, Image.Width, Image.Height);
            Color color;
            for (int y = 0; y < ImageWithPoints.Height; y++)
            {
                for (int x = 0; x < ImageWithPoints.Width; x++)
                {
                    if (InterestingPoints[y, x])
                    {
                        for (int hWinX = -r; hWinX <= r; hWinX++)
                        {
                            for (int hWinY = -r; hWinY <= r; hWinY++)
                            {
                                if (x + hWinX < ImageWithPoints.Width && x + hWinX >= 0
                                    && y + hWinY < ImageWithPoints.Height && y + hWinY >= 0)
                                {
                                    color = Color.FromArgb(255, 255, 0, 0);
                                    ImageWithPoints.Bitmap.SetPixel(x + hWinX, y + hWinY, color);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ANMS()
        {

        }
    }
}
