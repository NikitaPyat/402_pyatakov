using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace YOLOv4MLNet
{
    public class PictureInfo
    {
        Bitmap image { get; set; }
        string class_obj;
        Coordinate coordinate;

        public PictureInfo(Bitmap picture, string cl, Coordinate coord)
        {
            image = picture;
            class_obj = cl;
            coordinate = coord;
        }

        public Bitmap getImage()
        {
            return image;
        }

        public string getClass()
        {
            return class_obj;
        }

        public Coordinate Coordinate()
        {
            return coordinate;
        }
    }

}
