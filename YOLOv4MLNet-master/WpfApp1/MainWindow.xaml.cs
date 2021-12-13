using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YOLOv4MLNet;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<YOLOv4MLNet.PictureInfo> data = new List<YOLOv4MLNet.PictureInfo>();
        List<string> classes = new List<string>();
        CancellationTokenSource stop = new CancellationTokenSource();
        ArraySegment<PictureObject> classItems;
        string chosenClass = " ";
        string path_folder = @"Assets\Images";
        //IDictionary pictures = new Dictionary<string, string>();
        int picture_count = 0;
        int currentPicture = 1;
        int countClass;
        public MainWindow()
        {
            InitializeComponent();
            start_recognize.IsEnabled = true;
            using (var db = new PictureContext())
            {
                var query = db.PictureEntities;
                /*if (query.Any())
                {
                    foreach (var item in query)
                    {
                        classes.Add(item.label);
                    }
                    AfterRecognize();
                }*/
            }
        }

        private async void start(object sender, RoutedEventArgs e)
        {
            PictureInfo info;
            Recognition rec = new Recognition();
            ButtonsEnabled(false);
            button_back.IsEnabled = false;
            button_forward.IsEnabled = false;
            await Task.Factory.StartNew(() =>
            {
                rec.recognize(path_folder, stop);
                while (true)
                {
                    if (rec.queue.TryDequeue(out info))
                    {
                        if (info.getName() == " " || stop.IsCancellationRequested)
                        {
                            break;
                        }
                        else
                        {
                            picture_count++;
                            //data.Add(info);
                            //PictureInfo picture = data[data.Count() - 1];
                            string imageOutputFolder = "D:/Prak4/402_pyatakov/YOLOv4MLNet-master/WpfApp1/Output/";

                            var bitmap = new Bitmap(System.Drawing.Image.FromFile(System.IO.Path.Combine(path_folder, info.getName())));
                            var g = Graphics.FromImage(bitmap);
                            g.DrawRectangle(Pens.Red, (int)info.Coordinate().getX1(), (int)info.Coordinate().getY1(), (int)info.Coordinate().getX2minusX1(), (int)info.Coordinate().getY2minusY1());
                            using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                            {
                                g.FillRectangle(brushes, (int)info.Coordinate().getX1(), (int)info.Coordinate().getY1(), (int)info.Coordinate().getX2minusX1(), (int)info.Coordinate().getY2minusY1());
                            }

                            g.DrawString(info.getClass(), new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF((int)info.Coordinate().getX1(), (int)info.Coordinate().getY1()));
                            bitmap.Save(System.IO.Path.Combine(imageOutputFolder, System.IO.Path.ChangeExtension(picture_count.ToString(), System.IO.Path.GetExtension(info.getName()))));




                            if (!classes.Contains(info.getClass()))
                            {
                                classes.Add(info.getClass());
                            }

                            using (var db = new PictureContext())
                            {
                                var query = db.PictureEntities.Where(entity => entity.x1 == info.Coordinate().getX1() && entity.x2 == info.Coordinate().getX2() &&
                                entity.y1 == info.Coordinate().getY1() && entity.y2 == info.Coordinate().getY2());
                                if (query.Any())
                                {
                                    foreach (var item in query)
                                    {
                                        if (!Enumerable.SequenceEqual(item.picture, ImageToByte(bitmap)))
                                        {
                                            db.PictureEntities.Add(new PictureObject
                                            {
                                                x1 = info.Coordinate().getX1(),
                                                x2 = info.Coordinate().getX2(),
                                                y1 = info.Coordinate().getY1(),
                                                y2 = info.Coordinate().getY2(),
                                                picture = ImageToByte(bitmap),
                                                confidence = 0.95,
                                                label = info.getClass()
                                            });
                                            db.SaveChanges();
                                        }
                                    }
                                }
                                else
                                {
                                    db.PictureEntities.Add(new PictureObject
                                    {
                                        x1 = info.Coordinate().getX1(),
                                        x2 = info.Coordinate().getX2(),
                                        y1 = info.Coordinate().getY1(),
                                        y2 = info.Coordinate().getY2(),
                                        picture = ImageToByte(bitmap),
                                        confidence = 0.95,
                                        label = info.getClass()
                                    });
                                    db.SaveChanges();
                                }
                            }

                        }
                    }
                }
            }, stop.Token);

            AfterRecognize();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            stop.Cancel();
            AfterRecognize();
        }

        private void AfterRecognize()
        {
            ButtonsEnabled(true);

            for (int i = 0; i < classes.Count(); i++)
            {
                if (!labels.Items.Contains(classes[i]))
                {
                    labels.Items.Add(classes[i]);
                }
            }
        }

        private void ButtonsEnabled(Boolean recognize)
        {
            start_recognize.IsEnabled = recognize;
            stop_recognize.IsEnabled = !recognize;
            path.IsEnabled = recognize;
            labels.IsEnabled = recognize;
            choose_label.IsEnabled = recognize;
            makeempty.IsEnabled = recognize;
        }

        private void ChooseClass(object sender, RoutedEventArgs e)
        {
            if (labels.SelectedItem != null)
            {
                chosenClass = labels.SelectedItem.ToString();
                using (var db = new PictureContext())
                {
                    classItems = db.PictureEntities.Where(entity => entity.label == chosenClass).ToArray();
                    db.SaveChanges();
                }
                currentPicture = 0;
                countClass = classItems.Count;
                button_back.IsEnabled = false;
                button_forward.IsEnabled = false;
                forward(sender, e);
            }
        }

        private void back(object sender, RoutedEventArgs e)
        {
            while (currentPicture != 0)
            {
                currentPicture--;

                for (int i = 0; i < countClass; i++)
                {
                    if (i == currentPicture - 1)
                    {
                        image.Source = ToImage(classItems[i].picture);
                    }
                }
                break;
            }

            changeButtons();
        }

        private void forward(object sender, RoutedEventArgs e)
        {
            while (currentPicture != countClass)
            {
                currentPicture++;
                for (int i = 0; i < countClass; i++)
                {
                    if (i == currentPicture - 1)
                    {
                        image.Source = ToImage(classItems[i].picture);
                    }
                }
                break;
            }

            changeButtons();
        }

        private void changeButtons()
        {
            button_back.IsEnabled = currentPicture > 1;
            button_forward.IsEnabled = currentPicture != countClass;
        }

        private void ChoosePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                path_folder = openFileDialog.FileName;
        }

        public static byte[] ImageToByte(Bitmap img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private void MakeBaseEmpty(object sender, RoutedEventArgs e)
        {
            using (var db = new PictureContext())
            {
                var query = db.PictureEntities;
                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        db.Remove(item);
                    }
                    db.SaveChanges();

                    ButtonsEnabled(true);
                    button_back.IsEnabled = false;
                    button_forward.IsEnabled = false;
                    classes.Clear();
                    labels.Items.Clear();
                }
            }
        }
    }
}
