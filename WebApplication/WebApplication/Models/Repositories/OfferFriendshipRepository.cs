using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApplication.Interfaces;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models.Repositories
{
    public class OfferFriendshipRepository : IRepository<OfferFriendship>
    {
        private ApplicationDbContext _db;

        public OfferFriendshipRepository(ApplicationDbContext context)
        {
            this._db = context;
        }

        public IEnumerable<OfferFriendship> GetAll()
        {
            return _db.OfferFriendships;
        }

        public OfferFriendship GetById(int id)
        {
            return _db.OfferFriendships.Find(id);
        }

        public OfferFriendship GetById(string id) { return null; }

        public IEnumerable<OfferFriendship> Find(Func<OfferFriendship, Boolean> predicate)
        {
            return _db.OfferFriendships.Where(predicate).ToList();
        }

        public void Create(OfferFriendship photo)
        {
            _db.OfferFriendships.Add(photo);
        }

        public void Update(OfferFriendship photo)
        {
            _db.Entry(photo).State = EntityState.Modified;
        }

        public void Delete(long id)
        {
            OfferFriendship offerFriendship = _db.OfferFriendships.Find(id);
            if (offerFriendship != null)
                _db.OfferFriendships.Remove(offerFriendship);
        }
    }
}