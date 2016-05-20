using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public class Post
    {
        [Key]
        public virtual int Id
        { get; set; }

        public virtual string Title
        { get; set; }

        public virtual string ShortDescription
        { get; set; }

        public virtual string Description
        { get; set; }

        public virtual string Meta
        { get; set; }

        public virtual string UrlSlug
        { get; set; }

        public virtual bool Published
        { get; set; }

        public virtual DateTime PostedOn
        { get; set; }

        public virtual IList<Tag> Tags
        { get; set; }

        public int LikeCount
        { get; set; }

        public int CommentsCount
        { get; set; }

        public string Editor
        { get; set; }

        public string Lens
        { get; set; }
    }
}