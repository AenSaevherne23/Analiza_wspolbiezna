using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private const int _width = 800;
        private const int _height = 600;
        private PlotModel _plotModel;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureChart();
        }

        private void ConfigureChart()
        {
            _plotModel = new PlotModel { Title = "Chart" };
            var lineSeries = new LineSeries { Title = "Line Series" };
            _plotModel.Series.Add(lineSeries);
            plotView.Model = _plotModel;
        }

        private async void analyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (imagePreview.Source == null) return;

            await AnalyzeImageAsync();
        }

        private async Task AnalyzeImageAsync()
        {
            Bitmap orgBitmap = BitmapFromSource((BitmapSource)imagePreview.Source);
            double[] lineData = new double[orgBitmap.Width];

            for (int i = 0; i < orgBitmap.Height; i++)
            {
                Bitmap tempBitmap = new Bitmap(orgBitmap);
                for (int j = 0; j < orgBitmap.Width; j++)
                    lineData[j] = tempBitmap.GetPixel(j, i).R;

                Dispatcher.Invoke(() =>
                {
                    UpdateChart(lineData);
                    using (Graphics g = Graphics.FromImage(tempBitmap))
                    {
                        g.DrawLine(Pens.Black, 0, i, orgBitmap.Width, i);
                    }
                    var bitmapSource = BitmapToImageSource(tempBitmap);
                    imagePreview.Source = bitmapSource;
                });

                await Task.Delay(50);
            }
        }

        private void UpdateChart(double[] yValues)
        {
            var lineSeries = (LineSeries)_plotModel.Series[0];
            lineSeries.Points.Clear();
            for (int i = 0; i < yValues.Length; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, yValues[i]));
            }
            _plotModel.InvalidatePlot(true);
        }

        private void prepareButton_Click(object sender, RoutedEventArgs e)
        {
            Bitmap img = new Bitmap(_width, _height);
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    double cos = Math.Cos((i + j) * Math.PI / 180.0);
                    byte val = Convert.ToByte(255.0 * Math.Abs(cos));
                    img.SetPixel(i, j, Color.FromArgb(val, val, val));
                }
            }

            imagePreview.Source = BitmapToImageSource(img);
        }

        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}
