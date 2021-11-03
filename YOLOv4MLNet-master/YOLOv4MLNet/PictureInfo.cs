using System;
using System.Collections.Generic;
using System.Text;

namespace YOLOv4MLNet
{
    public class PictureInfo
    {
        string picture_name;
        string class_obj;
        Coordinate coordinate;

        public PictureInfo(string name, string cl, Coordinate coord)
        {
            picture_name = name;
            class_obj = cl;
            coordinate = coord;
        }

        public string getClass()
        {
            return class_obj;
        }

        public string getName()
        {
            return picture_name;
        }

        public Coordinate Coordinate()
        {
            return coordinate;
        }
    }

}
