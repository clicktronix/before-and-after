using System;
using WebApplication.Interfaces;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models.Repositories
{
    // класс, который содержит в себе все репозитории и передает им всем один контекст
    public class EfUnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private ApplicationUserRepository _applicationUserRepository;
        private EventRepository _eventRepository;
        private FriendshipRepository _friendshipRepository;
        private OfferFriendshipRepository _offerFriendshipRepository;

        public IRepository<ApplicationUser> Users
        {
            get { return _applicationUserRepository ?? (_applicationUserRepository = new ApplicationUserRepository(_db)); }
        }

        public IRepository<Event> Events
        {
            get { return _eventRepository ?? (_eventRepository = new EventRepository(_db)); }
        }
        public IRepository<Friendship> Friendships
        {
            get { return _friendshipRepository ?? (_friendshipRepository = new FriendshipRepository(_db)); }
        }

        public OfferFriendshipRepository OfferFriendships
        {
            get { return _offerFriendshipRepository ?? (_offerFriendshipRepository = new OfferFriendshipRepository(_db)); }
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        private bool _disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
                this._disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}