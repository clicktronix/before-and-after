using System.Collections.Generic;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models
{
    public class Gallery
    {
        public int GalleryId { get; set; }

        public string Id { get; set; }

        public virtual IList<Photo> Photos
        { get; set; }

        public ApplicationUser Creator { get; set; }
    }
}