using System;

namespace WebApplication.Models.DomainModels
{
    public class OfferFriendship
    {
        public long Id
        { set; get; }

        public ApplicationUser Offer
        { set; get; }

        public ApplicationUser Received
        { set; get; }

        public bool Status
        { set; get; }

        public DateTime Date
        { set; get; }
    }
}