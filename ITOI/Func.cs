using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITOI
{
    class Func
    {
        public Func()
        {

        }

        // Очистить директорию
        public void ClearDir(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
        }

        // Нормализовать матрицу к byte
        public byte[,] NormalizeMatrix(double[,] Matrix, int width, int height)
        {
            byte[,] ResultByte = new byte[height, width];
            double[,] Result = new double[height, width];

            double vMax = -999999999;
            double vMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = Matrix[y, x];
                    if (Result[y, x] >= vMax)
                    {
                        vMax = Result[y, x];
                    }
                    else if (Result[y, x] <= vMin)
                    {
                        vMin = Result[y, x];
                    }
                }
            }

            vMax -= vMin;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * 255.0 / vMax;
                    ResultByte[y, x] = Convert.ToByte(Result[y, x]);
                }
            }

            return ResultByte;
        }

        // Нормализовать матрицу
        public double[,] NormalizeMatrix(double[,] Matrix, int width, int height, double newMin, double newMax)
        {
            double[,] Result = new double[height, width];

            double vMax = -999999999;
            double vMin = 999999999;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = Matrix[y, x];
                    if (Result[y, x] >= vMax)
                    {
                        vMax = Result[y, x];
                    }
                    else if (Result[y, x] <= vMin)
                    {
                        vMin = Result[y, x];
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Result[y, x] = (Result[y, x] - vMin) * (newMax - newMin) / (vMax - vMin) + newMin;
                }
            }

            return Result;
        }

        // Преобразовать матрицу byte в битмап
        public Bitmap MatrixToImage(byte[,] GrayMatrix, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Color color;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = Color.FromArgb(255, GrayMatrix[y, x], GrayMatrix[y, x], GrayMatrix[y, x]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        // Преобразовать матрицу double в битмап
        public Bitmap MatrixToImage(double[,] GrayMatrix, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Color color;
            byte[,] NormMtx = NormalizeMatrix(GrayMatrix, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    color = Color.FromArgb(255, NormMtx[y, x], NormMtx[y, x], NormMtx[y, x]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        // Показать изображение
        public void DrawImage(Bitmap bitmap, PictureBox pictureBox)
        {
            pictureBox.Height = bitmap.Height;
            pictureBox.Width = bitmap.Width;
            pictureBox.Image = bitmap;

        }

        // Свёртка
        public double[,] Svertka(byte[,] GrayMatrix, int width, int height, double[,] mask, int k, int kraimode)
        {
            byte[,] GrayMatrixAdd = new byte[height + 2 * k, width + 2 * k];
            double[,] Result = new double[height, width];

            /* Краевые эффекты */
            // Копируем значение с края изображения
            if (kraimode == 1)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        GrayMatrixAdd[y + k, x + k] = GrayMatrix[y, x];
                    }
                }
                for (int y = k; y < height + k; y++)
                {
                    for (int x = 0; x < k; x++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[y, k];
                        GrayMatrixAdd[y, width + k + x] = GrayMatrixAdd[y, width + k - 1];
                    }
                }
                for (int x = 0; x < width + 2 * k; x++)
                {
                    for (int y = 0; y < k; y++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[k, x];
                        GrayMatrixAdd[height + k + y, x] = GrayMatrixAdd[height + k - 1, x];
                    }
                }
            }
            // Всё снаружи чёрное
            else
            {
                for (int y = 0; y < height + 2 * k; y++)
                {
                    for (int x = 0; x < width + 2 * k; x++)
                    {
                        if (x < k || y < k || y >= height + k || x >= width + k)
                        {
                            GrayMatrixAdd[y, x] = 0;
                        }
                        else
                        {
                            GrayMatrixAdd[y, x] = GrayMatrix[y - k, x - k];
                        }
                    }
                }
            }
            /*-----------------*/

            for (int y = k; y < height + k; y++)
            {
                for (int x = k; x < width + k; x++)
                {
                    double S = 0;
                    for (int hWinX = -k; hWinX <= k; hWinX++)
                    {
                        for (int hWinY = -k; hWinY <= k; hWinY++)
                        {
                            S += (double)GrayMatrixAdd[y - hWinY, x - hWinX] * mask[k + hWinY, k + hWinX];
                        }
                    }
                    Result[y - k, x - k] = S;
                }
            }

            return Result;
        }

        // Таблица Моравека
        public double[,] MoravekS(byte[,] GrayMatrix, int width, int height, int k/*Радиус окна*/, int kraimode)
        {
            int k1 = k + 1;
            byte[,] GrayMatrixAdd = new byte[height + 2 * k1, width + 2 * k1];
            double[,] Result = new double[height, width];

            /* Краевые эффекты */
            // Копируем значение с края изображения
            if (kraimode == 1)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        GrayMatrixAdd[y + k1, x + k1] = GrayMatrix[y, x];
                    }
                }
                for (int y = k1; y < height + k1; y++)
                {
                    for (int x = 0; x < k1; x++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[y, k1];
                        GrayMatrixAdd[y, width + k1 + x] = GrayMatrixAdd[y, width + k1 - 1];
                    }
                }
                for (int x = 0; x < width + 2 * k1; x++)
                {
                    for (int y = 0; y < k1; y++)
                    {
                        GrayMatrixAdd[y, x] = GrayMatrixAdd[k1, x];
                        GrayMatrixAdd[height + k1 + y, x] = GrayMatrixAdd[height + k1 - 1, x];
                    }
                }
            }
            // Всё снаружи чёрное
            else
            {
                for (int y = 0; y < height + 2 * k; y++)
                {
                    for (int x = 0; x < width + 2 * k; x++)
                    {
                        if (x < k || y < k || y >= height + k || x >= width + k)
                        {
                            GrayMatrixAdd[y, x] = 0;
                        }
                        else
                        {
                            GrayMatrixAdd[y, x] = GrayMatrix[y - k, x - k];
                        }
                    }
                }
            }
            /*-----------------*/

            for (int y = k1; y < height + k1; y++)
            {
                for (int x = k1; x < width + k1; x++)
                {
                    double S = 999999999;
                    for (int s = 1; s <= 8; s++)
                    {
                        int dx = 0;
                        int dy = 0;
                        if (s == 1)
                        {
                            dx = -1;
                            dy = -1;
                        }
                        else if (s == 2)
                        {
                            dx = -1;
                            dy = 0;
                        }
                        else if (s == 3)
                        {
                            dx = -1;
                            dy = 1;
                        }
                        else if (s == 4)
                        {
                            dx = 0;
                            dy = -1;
                        }
                        else if (s == 5)
                        {
                            dx = 0;
                            dy = 1;
                        }
                        else if (s == 6)
                        {
                            dx = 1;
                            dy = -1;
                        }
                        else if (s == 7)
                        {
                            dx = 1;
                            dy = 0;
                        }
                        else
                        {
                            dx = 1;
                            dy = 1;
                        }

                        double C = 0.0;
                        for (int hWinX = -k; hWinX <= k; hWinX++)
                        {
                            for (int hWinY = -k; hWinY <= k; hWinY++)
                            {
                                C += Math.Pow((double)GrayMatrixAdd[y + hWinY, x + hWinX] - (double)GrayMatrixAdd[y + hWinY + dx, x + hWinX + dy], 2);
                            }
                        }
                        if (C < S)
                        {
                            S = C;
                        }
                    }
                    Result[y - k1, x - k1] = S;
                }
            }

            return Result;
        }

    }
}
