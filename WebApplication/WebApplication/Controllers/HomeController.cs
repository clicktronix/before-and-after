using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Config;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Gallery(string filter = null, int page = 1, int pageSize = 20)
        {
            var records = new PagedList<Photo>();
            ViewBag.filter = filter;

            records.Content = _db.Photos
                        .Where(x => filter == null || (x.Description.Contains(filter)))
                        .OrderByDescending(x => x.PhotoId)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
            
            records.TotalRecords = _db.Photos
                            .Where(x => filter == null || (x.Description.Contains(filter))).Count();
            records.CurrentPage = page;
            records.PageSize = pageSize;

            return View(records);
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            var photo = new Photo();
            return View(photo);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Photo photo, IEnumerable<HttpPostedFileBase> files)
        {
            ImageConfig save = new ImageConfig();

            if (!ModelState.IsValid)
                return View(photo);
            if (files.Count() == 0 || files.FirstOrDefault() == null)
            {
                ViewBag.error = "Please choose a file";
                return View(photo);
            }

            var model = new Photo();
            foreach (var file in files)
            {
                if (file.ContentLength == 0) continue;

                model.Description = photo.Description;
                var fileName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(file.FileName).ToLower();

                using (var img = Image.FromStream(file.InputStream))
                {
                    model.ThumbPath = String.Format("/Files/thumbs/{0}{1}", fileName, extension);
                    model.ImagePath = String.Format("/Files/{0}{1}", fileName, extension);
                    
                    save.SaveToFolder(img, fileName, extension, new Size(100, 100), model.ThumbPath);
                    
                    save.SaveToFolder(img, fileName, extension, new Size(600, 600), model.ImagePath);
                }
                model.CreatedOn = DateTime.Now;
                _db.Photos.Add(model);
                _db.SaveChanges();
            }
            return PartialView();
        }
    }
}