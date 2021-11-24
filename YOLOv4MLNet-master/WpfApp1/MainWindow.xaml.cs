using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

using YOLOv4MLNet;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<YOLOv4MLNet.PictureInfo> data = new List<YOLOv4MLNet.PictureInfo>();
        List<string> classes = new List<string>();
        string chosenClass = " ";
        string path = @"Assets\Images";
        Dictionary<string, string> pictures = new Dictionary<string, string>();
        int picture_count = 0;
        int currentPicture = 1;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void start(object sender, RoutedEventArgs e)
        {
            PictureInfo info;
            Recognition rec = new Recognition();
            Task.Run(() => rec.recognize(path));
            while (true)
            {
                if (rec.queue.TryDequeue(out info))
                {
                    if (info.getName() == " ")
                    {
                        break;
                    }
                    else
                    {
                        picture_count++;
                        data.Add(info);
                        PictureInfo picture = data[data.Count() - 1];
                        string imageOutputFolder = "D:/Prak4/402_pyatakov/YOLOv4MLNet-master/WpfApp1/Output/";

                        var bitmap = new Bitmap(System.Drawing.Image.FromFile(System.IO.Path.Combine(path, picture.getName())));
                        var g = Graphics.FromImage(bitmap);
                        g.DrawRectangle(Pens.Red, (int)picture.Coordinate().getX1(), (int)picture.Coordinate().getY1(), (int)picture.Coordinate().getX2minusX1(), (int)picture.Coordinate().getY2minusY1());
                        using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                        {
                            g.FillRectangle(brushes, (int)picture.Coordinate().getX1(), (int)picture.Coordinate().getY1(), (int)picture.Coordinate().getX2minusX1(), (int)picture.Coordinate().getY2minusY1());
                        }

                        g.DrawString(picture.getClass(), new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF((int)picture.Coordinate().getX1(), (int)picture.Coordinate().getY1()));
                        bitmap.Save(System.IO.Path.Combine(imageOutputFolder, System.IO.Path.ChangeExtension(picture_count.ToString(), System.IO.Path.GetExtension(picture.getName()))));

                        pictures.Add(picture_count.ToString() + ".jpg", picture.getClass());


                        if (!classes.Contains(data[data.Count() - 1].getClass()))
                        {
                            classes.Add(data[data.Count() - 1].getClass());
                        }
                        Console.WriteLine(info.getName() + " " + info.getClass());
                    }
                }
            }

            for (int i = 0; i < classes.Count(); i++)
            {
                comboBox.Items.Add(classes[i]);
            }
        }

        private void ChooseClass(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem != null)
            {
                chosenClass = comboBox.SelectedItem.ToString();
                currentPicture = 0;
                forward(sender, e);
            }
        }

        private void back(object sender, RoutedEventArgs e)
        {
            while(currentPicture != 1)
            {
                currentPicture--;
                if (pictures[currentPicture.ToString() + ".jpg"] == chosenClass)
                {
                    image.Source = new BitmapImage(new Uri("D:/Prak4/402_pyatakov/YOLOv4MLNet-master/WpfApp1/Output/" + currentPicture.ToString() + ".jpg"));
                    break;
                }
            }
        }

        private void forward(object sender, RoutedEventArgs e)
        {
            while (currentPicture != pictures.Count())
            {
                currentPicture++;
                if (pictures[currentPicture.ToString() + ".jpg"] == chosenClass)
                {
                    image.Source = new BitmapImage(new Uri("D:/Prak4/402_pyatakov/YOLOv4MLNet-master/WpfApp1/Output/" + currentPicture.ToString() + ".jpg"));
                    break;
                }
            }
        }

        private void ChoosePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                path = openFileDialog.FileName;
        }
    }
}
