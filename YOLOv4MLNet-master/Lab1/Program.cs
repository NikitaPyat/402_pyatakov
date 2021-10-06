using System;
using YOLOv4MLNet;

namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            Recognition.Recognize(args[0]);
        }
    }
}
