using System.Windows.Forms;

namespace App20241224
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "�Ϲ����(JPeg, Gif, Bmp, etc.)|.jpg;*jpeg;*.gif;*.bmp;*.tif;*.tiff;*.png|�Ҧ����(*.*)|*.*";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Bitmap MyBitmap = new Bitmap(openFileDialog1.FileName);
                    this.pictureBox1.Image = MyBitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "�T�����");
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    MessageBox.Show("�Х����J�v���I");
                    return;
                }

                int threshold = 100; // �w�]���e��
                if (!int.TryParse(textBox1.Text, out threshold) || threshold < 0 || threshold > 255)
                {
                    MessageBox.Show("�п�J���Ī����e�ȡ]0-255�^�I");
                    return;
                }

                Bitmap oldBitmap = (Bitmap)pictureBox1.Image;
                Bitmap edgeBitmap = PerformEdgeDetection(oldBitmap, threshold);
                pictureBox2.Image = edgeBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "���~");
            }
        }

        private Bitmap PerformEdgeDetection(Bitmap oldBitmap, int threshold)
        {
            int[,] PrewittGx = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            int[,] PrewittGy = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

            int Width = oldBitmap.Width;
            int Height = oldBitmap.Height;
            Bitmap edgeBitmap = new Bitmap(Width, Height);

            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {
                    int Gx = 0, Gy = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int gray = (int)(oldBitmap.GetPixel(x + i, y + j).R * 0.299 +
                                             oldBitmap.GetPixel(x + i, y + j).G * 0.587 +
                                             oldBitmap.GetPixel(x + i, y + j).B * 0.114);
                            Gx += PrewittGx[i + 1, j + 1] * gray;
                            Gy += PrewittGy[i + 1, j + 1] * gray;
                        }
                    }

                    double magnitude = Math.Sqrt(Gx * Gx + Gy * Gy);
                    if (magnitude >= threshold)
                        edgeBitmap.SetPixel(x, y, Color.White);
                    else
                        edgeBitmap.SetPixel(x, y, Color.Black);
                }
            }

            return edgeBitmap;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap sobelImage = new Bitmap(pictureBox2.Image);
            Bitmap thinnedImage = ZhangSuenThinning(sobelImage);
            pictureBox3.Image = thinnedImage;
        }

        private Bitmap ZhangSuenThinning(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            bool pixel_remove;

            short[,] img = new short[width, height];

            // �M���v���ন(��)��(�D��)���}�C
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    img[x, y] = (short)(image.GetPixel(x, y).R == 255 ? 1 : 0);
                }
            }

            do
            {
                pixel_remove = false;
                bool[,] remove = new bool[width, height];

                // �����1�ˬd�A�p�⶷�������乳���y��
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        if (img[x, y] == 1 && Check1(img, x, y))
                        {
                            remove[x, y] = true;
                            pixel_remove = true;
                        }
                    }
                }
                if (!pixel_remove) break;

                // �������w���乳��
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (remove[x, y])
                        {
                            img[x, y] = 0;
                        }
                    }
                }

                pixel_remove = false;
                remove = new bool[width, height];

                // �����2�ˬd�A�p�⶷�������乳���y��
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        if (img[x, y] == 1 && Check2(img, x, y))
                        {
                            remove[x, y] = true;
                            pixel_remove = true;
                        }
                    }
                }
                if (!pixel_remove) break;

                // �������w���乳��
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (remove[x, y])
                        {
                            img[x, y] = 0;
                        }
                    }
                }
            } while (pixel_remove);

            Bitmap result = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result.SetPixel(x, y, img[x, y] == 1 ? Color.White : Color.Black);
                }
            }
            return result;
        }

        private bool Check2(short[,] img, int x, int y)
        {
            int B = Count_NonZeroNeighbors(img, x, y);
            int A = Count_Otol(img, x, y);
            return B >= 2 && B <= 6 && A == 1 &&
                   img[x - 1, y] * img[x, y + 1] * img[x, y - 1] == 0 &&
                   img[x - 1, y] * img[x + 1, y] * img[x, y - 1] == 0;
        }



        private bool Check1(short[,] img, int x, int y)
        {
            int B = Count_NonZeroNeighbors(img, x, y);
            int A = Count_Otol(img, x, y);
            return B >= 2 && B <= 6 && A == 1 &&
                   img[x - 1, y] * img[x, y + 1] * img[x + 1, y] == 0 &&
                   img[x, y + 1] * img[x + 1, y] * img[x, y - 1] == 0;
        }

        private int Count_Otol(short[,] img, int x, int y)
        {
            int[] neighbors = {
        img[x - 1, y], img[x - 1, y + 1], img[x, y + 1], img[x + 1, y + 1],
        img[x + 1, y], img[x + 1, y - 1], img[x, y - 1], img[x - 1, y - 1]
    };

            int transition_Otol = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i] == 0 && neighbors[(i + 1) % neighbors.Length] == 1)
                {
                    transition_Otol++;
                }
            }
            return transition_Otol;
        }

        private int Count_NonZeroNeighbors(short[,] img, int x, int y)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        count += img[x + i, y + j];
                    }
                }
            }
            return count;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null)
            {
                MessageBox.Show("���ӽu�Ƥ~���x�s�C");
                return;
            }

            // �ϥ� SaveFileDialog ����x�s���|
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image Files(png)|*.png";
            saveFileDialog.Title = "�x�s�ӽu�ƹϹ�";
            saveFileDialog.FileName = "Thinned_Edge_Result";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // �x�s�ӽu�ƹϹ�
                Bitmap thinnedResult = new Bitmap(pictureBox3.Image);
                thinnedResult.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("�x�s�ӽu�ƹϹ����\�I");
            }
        }
    }
}
