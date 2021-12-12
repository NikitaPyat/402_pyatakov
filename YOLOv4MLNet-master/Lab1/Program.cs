﻿using YOLOv4MLNet;
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
            CancellationTokenSource stop = new CancellationTokenSource();
            Recognition rec = new Recognition();
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
        }
    }
}
