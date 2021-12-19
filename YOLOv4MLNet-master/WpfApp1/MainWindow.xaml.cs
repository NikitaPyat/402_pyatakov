using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        CancellationTokenSource stop = new CancellationTokenSource();
        List<PictureObject> classItems = new List<PictureObject>();
        string chosenClass = " ";
        string path_folder = @"Assets\Images";
        int picture_count = 0;
        int currentPicture = 1;
        int countClass;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public MainWindow()
        {
            InitializeComponent();
            labels.Items.Clear();
            GetPhotos();
        }

        private async void GetPhotos()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);

            string json = "";
            try
            {
                json = await client.GetStringAsync("http://localhost:5000/Pictures");

                PictureObject[] deserializedPictures = JsonConvert.DeserializeObject<PictureObject[]>(json);
                for (int i = 0; i < deserializedPictures.Length; i++)
                {
                    if (!labels.Items.Contains(deserializedPictures[i].label))
                    {
                        labels.Items.Add(deserializedPictures[i].label);
                    }
                }

                AfterRecognize();
            }
            catch (Exception e)
            {
                return;
            }
        }

        private async void start(object sender, RoutedEventArgs e)
        {
            var imageName = new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg" };

            ButtonsEnabled(false);
            button_back.IsEnabled = false;
            button_forward.IsEnabled = false;
            var tasks = new Task[imageName.Length];

            for (int i = 0; i < imageName.Length; i++)
            {
                if (stop.IsCancellationRequested) return;
                tasks[i] = Task.Factory.StartNew(async pi => {
                    if (stop.IsCancellationRequested)
                    {
                        return;
                    }
                    int idx = (int)pi;

                    HttpClientHandler clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                    HttpClient client = new HttpClient(clientHandler);
                    var bitmap = new Bitmap(Image.FromFile(Path.Combine(path_folder, imageName[idx])));
                    var bytes = ImageToByte(bitmap);

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(bytes);
                    var data = new System.Net.Http.StringContent(json, Encoding.Default, "application/json");
                    HttpResponseMessage response = new();
                    response = await client.PostAsync("http://localhost:5000/Pictures/Add", data);
                    MessageBox.Show(response.ToString());
                }, i, stop.Token);
            }
            Task.WaitAll(tasks);
            AfterRecognize();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            stop.Cancel();
            AfterRecognize();
        }

        private async void AfterRecognize()
        {
            ButtonsEnabled(true);

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            string json = "";
            HttpClient client = new HttpClient(clientHandler);
            try
            {
                json = await client.GetStringAsync("http://localhost:5000/Pictures");
                //int j = 0;
                PictureObject[] deserializedPictures = JsonConvert.DeserializeObject<PictureObject[]>(json);
                for (int i = 0; i < deserializedPictures.Length; i++)
                {
                    if (!labels.Items.Contains(deserializedPictures[i].label))
                    {
                        labels.Items.Add(deserializedPictures[i].label);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void ButtonsEnabled(bool recognize)
        {
            start_recognize.IsEnabled = recognize;
            stop_recognize.IsEnabled = !recognize;
            path.IsEnabled = recognize;
            labels.IsEnabled = recognize;
            choose_label.IsEnabled = recognize;
            makeempty.IsEnabled = recognize;
        }

        private async void ChooseClass(object sender, RoutedEventArgs e)
        {
            classItems.Clear();
            if (labels.SelectedItem != null)
            {
                chosenClass = labels.SelectedItem.ToString();
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                string json = "";
                HttpClient client = new HttpClient(clientHandler);
                try
                {
                    json = await client.GetStringAsync("http://localhost:5000/Pictures");
                    //int j = 0;
                    PictureObject[] deserializedPictures = JsonConvert.DeserializeObject<PictureObject[]>(json);
                    for (int i = 0; i < deserializedPictures.Length; i++)
                    {
                        if (chosenClass == deserializedPictures[i].label)
                        {
                            classItems.Add(deserializedPictures[i]);
                        }
                    }
                }
                catch (Exception)
                {
                    return;
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

        private async void MakeBaseEmpty(object sender, RoutedEventArgs e)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            string json = "";
            HttpClient client = new HttpClient(clientHandler);
            try
            {
                var r = await client.DeleteAsync("http://localhost:5000/Pictures/clean");
                r.EnsureSuccessStatusCode();
                ButtonsEnabled(true);
                button_back.IsEnabled = false;
                button_forward.IsEnabled = false;
                labels.Items.Clear();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
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
            Bitmap bmp;
            using (var ms = new MemoryStream(array))
            {
                bmp = new Bitmap(ms);
            }
            using (var memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
