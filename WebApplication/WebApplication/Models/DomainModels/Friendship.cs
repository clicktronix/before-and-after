using System;

namespace WebApplication.Models.DomainModels
{
    public class Friendship
    {
        public long FriendshipId
        { set; get; }

        public ApplicationUser FirstUser
        { set; get; }

        public ApplicationUser SecondUser
        { set; get; }

        public DateTime DateOfCreate
        { set; get; }

        public DateTime? DateOfDelete
        { set; get; }
    }
}