using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    public class Gallery
    {
        [Key]
        public int Id { get; set; }

        public int GalleryId { get; set; }

        private ICollection<Photo> Photos { get; set; }
    }
}