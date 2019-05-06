using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KursProj
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

        // Нормализовать вектор
        public double[] NormalizeVector(double[] Vector, int size, double newMin, double newMax)
        {
            double[] Result = new double[size];

            double vMax = -999999999;
            double vMin = 999999999;
            for (int i = 0; i < size; i++)
            {
                Result[i] = Vector[i];
                if (Result[i] >= vMax)
                {
                    vMax = Result[i];
                }
                else if (Result[i] <= vMin)
                {
                    vMin = Result[i];
                }
            }

            for (int i = 0; i < size; i++)
            {
                Result[i] = (Result[i] - vMin) * (newMax - newMin) / (vMax - vMin) + newMin;
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

        // Свёртка
        public double[,] Svertka(double[,] GrayMatrix, int width, int height, double[,] mask, int k, int kraimode)
        {
            double[,] GrayMatrixAdd = new double[height + 2 * k, width + 2 * k];
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
                            S += GrayMatrixAdd[y - hWinY, x - hWinX] * mask[k + hWinY, k + hWinX];
                        }
                    }
                    Result[y - k, x - k] = S;
                }
            }

            return Result;
        }

        // Сдвиг по x, y
        public byte[,] Sdvig(byte[,] GrayMatrix, int width, int height, out int newwidth, out int newheight, int dx, int dy)
        {
            newwidth = width + dx;
            newheight = height + dy;
            byte[,] Result = new byte[newheight, newwidth];
            for (int y = 0; y < newheight; y++)
            {
                for (int x = 0; x < newwidth; x++)
                {
                    Result[y, x] = 255;
                }
            }

            for (int y = 0; y < newheight; y++)
            {
                for (int x = 0; x < newwidth; x++)
                {
                    if (y - dy >= 0 && y - dy < height
                        && x - dx >=0 && x - dx < width)
                    {
                        Result[y, x] = GrayMatrix[y - dy, x - dx];
                    }
                }
            }

            return Result;
        }

        // Поворот
        public Bitmap RotateImage(Bitmap input, float angle)
        {
            Bitmap result = new Bitmap(input.Width, input.Height);
            Graphics g = Graphics.FromImage(result);
            g.TranslateTransform((float)input.Width / 2, (float)input.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-(float)input.Width / 2, -(float)input.Height / 2);
            for (int y = 0; y < input.Width; y++)
            {
                for (int x = 0; x < input.Height; x++)
                {
                    result.SetPixel(x, y, Color.White);
                }
            }
            g.DrawImage(input, new Point(0, 0));
            return result;
        }

        // Шум
        public double[,] Noise(byte[,] GrayMatrix, int width, int height, int intensive)
        {
            double[,] Result = new double[height, width];
            Random Rnd = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double noise = (Rnd.NextDouble() + Rnd.NextDouble() + Rnd.NextDouble() + Rnd.NextDouble() - 2) * intensive;
                    Result[y, x] = GrayMatrix[y, x] + noise;
                }
            }

            return Result;
        }

        // Яркость
        public byte[,] Brightness(byte[,] GrayMatrix, int width, int height, int intensive)
        {
            if (intensive > 255)
            {
                intensive = 255;
            }
            else if (intensive < -255)
            {
                intensive = -255;
            }
            byte[,] BrightMtx = new byte[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int u = GrayMatrix[y, x] + intensive;
                    if (u > 255)
                    {
                        u = 255;
                    }
                    else if (u < 0)
                    {
                        u = 0;
                    }
                    BrightMtx[y, x] = Convert.ToByte(u);
                }
            }
            return BrightMtx;
        }

        // Контрастность
        public byte[,] Contrast(byte[,] GrayMatrix, int width, int height, int intensive)
        {
            if (intensive > 100)
            {
                intensive = 100;
            }
            else if (intensive < -100)
            {
                intensive = -100;
            }
            double contrast = (100.0 + intensive) / 100.0;
            contrast = contrast * contrast;
            byte[,] Res = new byte[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double u = GrayMatrix[y, x] / 255.0;
                    u = u - 0.5;
                    u = u * contrast;
                    u = u + 0.5;
                    u = u * 255;
                    if (u > 255)
                    {
                        u = 255;
                    }
                    else if (u < 0)
                    {
                        u = 0;
                    }
                    Res[y, x] = Convert.ToByte(u);
                }
            }
            return Res;
        }

    }
}
