using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using YOLOv4MLNet;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PicturesController : ControllerBase
    {
        private readonly ILogger<PicturesController> _logger;

        private PictureContext db;

        private byte[] image;

        public PicturesController(ILogger<PicturesController> logger, PictureContext db)
        {
            _logger = logger;
            this.db = db;
        }

        [HttpGet]
        public IEnumerable<PictureObject> GetPictures()
        {
            return db.PictureEntities;
        }

        [HttpPost("Add")]
        public ActionResult Add([FromBody] byte[] bytes)
        {
            Recognition rec = new Recognition();
            var imageName = new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg" };
            PictureInfo info;
            CancellationTokenSource stop = new CancellationTokenSource();

            rec.recognize(ToImage(bytes), stop);

            while (true)
            {
                if (rec.queue.TryDequeue(out info))
                {
                    if (info.getClass() == " " || stop.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {

                        var bitmap = new Bitmap(info.getImage());
                        var g = Graphics.FromImage(bitmap);
                        g.DrawRectangle(Pens.Red, (int)info.Coordinate().getX1(), (int)info.Coordinate().getY1(), (int)info.Coordinate().getX2minusX1(), (int)info.Coordinate().getY2minusY1());
                        using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                        {
                            g.FillRectangle(brushes, (int)info.Coordinate().getX1(), (int)info.Coordinate().getY1(), (int)info.Coordinate().getX2minusX1(), (int)info.Coordinate().getY2minusY1());
                        }

                        g.DrawString(info.getClass(), new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF((int)info.Coordinate().getX1(), (int)info.Coordinate().getY1()));


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
            };
            return StatusCode(200, "ok");
        }

        [HttpDelete("clean")]
        public void Delete()
        {
            var query = db.PictureEntities;
            if (query.Any())
            {
                foreach (var item in query)
                {
                    db.PictureEntities.Remove(item);
                }
                db.SaveChanges();
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

        public Bitmap ToImage(byte[] array)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(array))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }
    }
}
