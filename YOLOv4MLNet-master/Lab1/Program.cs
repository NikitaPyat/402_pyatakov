using YOLOv4MLNet;
using System;
using System.Collections.Generic;

namespace Lab1
{
    class Program
    {
        Queue<string> result = new Queue<string>();
        static void Main(string[] args)
        {
            string path = @"Assets\Images";
            Queue<string> result = Recognition.Recognize(path);

            while( result.Count != 0)
            {
                Console.WriteLine(result.Dequeue());
            }
        }
    }
}
