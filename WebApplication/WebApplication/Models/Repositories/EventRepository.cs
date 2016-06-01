using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApplication.Interfaces;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models.Repositories
{
    public class EventRepository : IRepository<Event>
    {
        private ApplicationDbContext _db;

        public EventRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public IEnumerable<Event> GetAll()
        {
            return _db.Events.Include(u => u.Owners);
        }

        public Event GetById(int id)
        {
            return _db.Events.Find(id);
        }

        public Event GetById(string id) { return null; }

        public IEnumerable<Event> Find(Func<Event, Boolean> predicate)
        {
            return _db.Events.Where(predicate).ToList();
        }

        public void Create(Event photo)
        {
            _db.Events.Add(photo);
        }

        public void Update(Event photo)
        {
            _db.Entry(photo).State = EntityState.Modified;
        }

        public void Delete(long id)
        {
            Event Event = _db.Events.Find(id);

            if (Event != null)
                _db.Events.Remove(Event);
        }
    }
}