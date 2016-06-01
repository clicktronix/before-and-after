using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApplication.Interfaces;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models.Repositories
{
    public class FriendshipRepository : IRepository<Friendship>
    {
        private ApplicationDbContext _db;

        public FriendshipRepository(ApplicationDbContext context)
        {
            this._db = context;
        }

        public IEnumerable<Friendship> GetAll()
        {
            return _db.Friendships.Include(f => f.FirstUser).Include(f => f.SecondUser).ToList();
        }

        public Friendship GetById(int id)
        {
            return _db.Friendships.Find(id);
        }

        public Friendship GetById(string id) { return null; }

        public IEnumerable<Friendship> Find(Func<Friendship, Boolean> predicate)
        {
            return _db.Friendships.Where(predicate).ToList();
        }

        public void Create(Friendship photo)
        {
            _db.Friendships.Add(photo);
        }

        public void Update(Friendship photo)
        {
            _db.Entry(photo).State = EntityState.Modified;
        }

        public void Delete(long id)
        {
            Friendship friendship = _db.Friendships.Find(id);
            if (friendship != null)
                _db.Friendships.Remove(friendship);
        }
    }
}