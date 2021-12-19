using YOLOv4MLNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    class Program
    {
        static string path = @"Assets\Images";
        static void Main(string[] args)
        {
            /*var imageName = new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg" };
            CancellationTokenSource stop = new CancellationTokenSource();
            Recognition rec = new Recognition();
            var tasks = new Task[imageName.Length];

            for (int i = 0; i < imageName.Length; i++)
            {
                if (stop.IsCancellationRequested) return;
                tasks[i] = Task.Factory.StartNew(async pi => {
                    if (stop.IsCancellationRequested)
                    {
                        return;
                    }
                    rec.recognize(, stop);
                //MessageBox.Show(response.ToString());
            }, i, stop.Token);
            }
            Task.WaitAll(tasks);






            Task.Run(() => rec.recognize(path, stop));
            PictureInfo info;
            while (true)
            {
                if (rec.queue.TryDequeue(out info))
                {
                    if (info.getName() == " ")
                    {
                        break;
                    } else 
                    {
                        Console.WriteLine(info.getName() + " " + info.getClass());
                    }
                }
            }
        */
        }
    }
}
