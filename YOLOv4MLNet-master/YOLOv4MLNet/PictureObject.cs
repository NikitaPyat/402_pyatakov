using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YOLOv4MLNet
{
    public class PictureObject
    {
        public int ID { get; set; }
        public double x1 { get; set; }
        public double y1 { get; set; }
        public double x2 { get; set; }
        public double y2 { get; set; }
        [ConcurrencyCheck]
        public byte[] picture { get; set; }
        public double confidence { get; set; }
        public string label { get; set; }


    }
}
