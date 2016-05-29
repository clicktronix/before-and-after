using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    public class Gallery
    {
        public int GalleryId { get; set; }

        public string Id { get; set; }

        public virtual IList<Photo> Photos
        { get; set; }
    }
}