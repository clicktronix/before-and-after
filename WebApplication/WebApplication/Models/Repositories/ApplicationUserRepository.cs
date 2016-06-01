using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebApplication.Interfaces;

namespace WebApplication.Models.Repositories
{
    public class ApplicationUserRepository : IRepository<ApplicationUser>
    {
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> _store;
        private ApplicationUserManager _manager;

        public ApplicationUserRepository(ApplicationDbContext context)
        {
            this._context = context;
            _store = new UserStore<ApplicationUser>(context);
            _manager = new ApplicationUserManager(_store);
        }

        public IEnumerable<ApplicationUser> GetAll()
        {
            return _manager.Users;
        }

        public ApplicationUser GetById(int id) { return null; }

        public ApplicationUser GetById(string id)
        {
            return _manager.Users.FirstOrDefault(u => u.Id == id);
        }

        public IEnumerable<ApplicationUser> Find(Func<ApplicationUser, Boolean> predicate)
        {
            return _manager.Users.Where(predicate).ToList();
        }

        public void Create(ApplicationUser photo){}

        public void Update(ApplicationUser photo)
        {
            _context.Set<ApplicationUser>().AddOrUpdate(photo);
            _context.SaveChanges();
        }

        public void Delete(long id) { }

        public void Delete(string id)
        {
            ApplicationUser user = _manager.Users.FirstOrDefault(u => u.Id != null && u.Id == id);

            if (user != null)
                _manager.Delete(user);
        }
    }
}