using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApplication.Interfaces;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models.Repositories
{
    public class PhotoRepository : IRepository<Photo>
    {
        private ApplicationDbContext _db;

        public PhotoRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public IEnumerable<Photo> GetAll()
        {
            return _db.Photos;
        }

        public Photo GetById(int id)
        {
            return _db.Photos.Find(id);
        }

        public Photo GetById(string id) { return null; }

        public IEnumerable<Photo> Find(Func<Photo, Boolean> predicate)
        {
            return _db.Photos.Where(predicate).ToList();
        }

        public void Create(Photo photo)
        {
            _db.Photos.Add(photo);
        }

        public void Update(Photo photo)
        {
            _db.Entry(photo).State = EntityState.Modified;
        }

        public void Delete(long id)
        {
            Photo photo = _db.Photos.Find(id);
            if (photo != null)
                _db.Photos.Remove(photo);
        }
    }
}