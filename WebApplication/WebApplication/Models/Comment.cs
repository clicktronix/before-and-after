using System;

namespace WebApplication.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public string CommentText { get; set; }

        public int UpdateId { get; set; }

        public DateTime CommentDate { get; set; }

        public Comment()
        {
            CommentDate = DateTime.Now;
        }
    }
}