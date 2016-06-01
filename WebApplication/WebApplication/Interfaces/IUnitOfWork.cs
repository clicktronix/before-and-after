using System;
using WebApplication.Models;
using WebApplication.Models.DomainModels;
using WebApplication.Models.Repositories;

namespace WebApplication.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ApplicationUser> Users { get; }
        IRepository<Event> Events { get; }
        IRepository<Friendship> Friendships { get; }
        OfferFriendshipRepository OfferFriendships { get; }
        void Save();
    }
}