using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace YOLOv4MLNet
{
    public class PictureContext: DbContext
    {
        public DbSet<PictureObject> PictureEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=D:\\Prak4\\402_pyatakov\\YOLOv4MLNet-master\\YOLOv4MLNet\\pictureBase.db");
        }
    }
}
