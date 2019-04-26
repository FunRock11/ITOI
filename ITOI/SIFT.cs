using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITOI
{
    class SIFT
    {
        public string BasePath = "../../../files/";
        public Img BeginImage;
        public Img[,] Piramida;
        public Img[,] DoG;
        public int HarrisRadius;
        public double HarrisPorog;
        public int NPoints;
        public int S; // Число масштабов в октаве
        public int O; // Число октав
        public double[,] GlobalSigma;
        public double[,] Sigma;

        public List<InterestingPoints> InterestingPoints;

        public SIFT(Img begimage, int harrisradius, double harrisporog, int npoins, int s)
        {
            BeginImage = new Img(begimage.Bitmap);
            HarrisRadius = harrisradius;
            HarrisPorog = harrisporog;
            NPoints = npoins;
            S = s; 

            Piramids();
            FindPoints();
            
        }

        private void Piramids()
        {
            double sigma0 = 1.6; // sigma 0
            double k = Math.Pow(2.0, (1.0 / (double)S)); // Интервал между масштабами

            int minr = Math.Min(BeginImage.Width, BeginImage.Height);
            O = 0;
            while (minr > 32)
            {
                minr /= 2;
                O++;
            }

            Piramida = new Img[O + 1, S + 3];
            DoG = new Img[O + 1, S + 2];

            Img TekImg = new Img(BeginImage.Bitmap);
            double sigmaD = sigma0;                        // Действительная сигма
            double sigmaTEK = sigma0;
            int qq = 1;

            GaussCore GaussMatrix = new GaussCore(sigma0);
            TekImg.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
            Piramida[0, 0] = new Img(TekImg.Bitmap);

            GlobalSigma = new double[O + 1, S + 3];
            Sigma = new double[O + 1, S + 3];


            Sigma[0, 0] = sigmaTEK;
            GlobalSigma[0, 0] = sigmaD;

            for (int o = 0; o < O + 1; o++)
            {
                TekImg = new Img(Piramida[o, 0].Bitmap);
                for (int s = 1; s < S + 3; s++)
                {
                    double sigma1 = sigma0 * Math.Pow(k, (s - 1));
                    double sigma2 = sigma0 * Math.Pow(k, s);
                    sigmaTEK = Math.Sqrt(sigma2 * sigma2 - sigma1 * sigma1); // Текущая сигма
                    Sigma[o, s] = sigma2;
                    sigmaD = sigma0 * Math.Pow(k, qq);
                    GlobalSigma[o, s] = sigmaD;
                    qq++;
                    GaussMatrix = new GaussCore(sigmaTEK);
                    TekImg.SvertkaWithNormalize(GaussMatrix.Matrix, GaussMatrix.Radius, 1);
                    Piramida[o, s] = new Img(TekImg.Bitmap);

                    if ((o + 1) != (O + 1) && s == S)
                    {
                        Piramida[o + 1, 0] = new Img(TekImg.Bitmap);
                        Piramida[o + 1, 0].Downsample();

                        Sigma[o + 1, 0] = sigma0;
                        GlobalSigma[o + 1, 0] = sigmaD;
                    }

                }
                qq = qq - 2;
            }

            for (int o = 0; o < O + 1; o++)
            {
                for (int s = 0; s < S + 2; s++)
                {
                    double[,] RMtx = new double[Piramida[o, 0].Height, Piramida[o, 0].Width];
                    for (int y = 0; y < Piramida[o, 0].Height; y++)
                    {
                        for (int x = 0; x < Piramida[o, 0].Width; x++)
                        {
                            RMtx[y, x] = Math.Abs(Piramida[o, s + 1].GrayMatrixDouble[y, x] - Piramida[o, s].GrayMatrixDouble[y, x]);
                        }
                    }
                    DoG[o, s] = new Img(RMtx, Piramida[o, 0].Width, Piramida[o, 0].Height);
                    DoG[o, s].Save(BasePath + "Lab 6/i" + Convert.ToString(o) + Convert.ToString(s) + ".png");
                }
            }

        }

        private void FindPoints()
        {
            InterestingPoints = new List<InterestingPoints>();
            for (int o = 0; o < O + 1; o++)
            {
                for (int s = 1; s < S + 1; s++)
                {
                    for (int y = 1; y < DoG[o, 0].Height - 1; y++)
                    {
                        for (int x = 1; x < DoG[o, 0].Width - 1; x++)
                        {
                            bool Tr = true;
                            string znak = "EQ";
                            if (DoG[o, s].GrayMatrixDouble[y, x] > DoG[o, s].GrayMatrixDouble[y - 1, x - 1])
                            {
                                znak = "GT";
                            }
                            else if (DoG[o, s].GrayMatrixDouble[y, x] < DoG[o, s].GrayMatrixDouble[y - 1, x - 1])
                            {
                                znak = "LT";
                            }

                            if (znak == "EQ")
                            {
                                Tr = false;
                                goto L1;
                            }
                            else
                            {
                                for (int dl = -1; dl <= 1; dl++)
                                {
                                    for (int dy = -1; dy <= 1; dy++)
                                    {
                                        for (int dx = -1; dx <= 1; dx++)
                                        {
                                            if (dl == 0 && dx == 0 && dy == 0)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (znak == "GT")
                                                {
                                                    if (DoG[o, s].GrayMatrixDouble[y, x] <= DoG[o, s + dl].GrayMatrixDouble[y + dx, x + dy])
                                                    {
                                                        Tr = false;
                                                        goto L1;
                                                    }
                                                }
                                                else if (znak == "LT")
                                                {
                                                    if (DoG[o, s].GrayMatrixDouble[y, x] >= DoG[o, s + dl].GrayMatrixDouble[y + dx, x + dy])
                                                    {
                                                        Tr = false;
                                                        goto L1;
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }

                        L1:
                            if (Tr)
                            {
                                InterestingPoints TekPoint;
                                TekPoint = new InterestingPoints(x * Convert.ToInt32(Math.Round(Math.Pow(2, o))), y * Convert.ToInt32(Math.Round(Math.Pow(2, o))), GlobalSigma[o, s], Sigma[o, s], o, s);
                                

                                InterestingPoints.Add(TekPoint);
                            }

                        }
                    }
                }
            }
        }
    }
}
