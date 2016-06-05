using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models
{
    // Чтобы добавить данные профиля для пользователя, можно добавить дополнительные свойства в класс ApplicationUser. 
    //Дополнительные сведения см. по адресу: http://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public int PostsCount { get; set; }

        public DateTime? Age { get; set; }

        public bool? Gender { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Avatar { set; get; }

        public string Status { set; get; }
        
        public DateTime? DateOfActivity { set; get; }


        public virtual ICollection<Photo> Photos { get; set; }

        public ICollection<Event> Events { set; get; }

        public ICollection<OfferFriendship> OfferFriendships { set; get; }

        public ICollection<Friendship> Friendships { set; get; }

        public ApplicationUser()
        {
            DateOfActivity = DateTime.Now;
            Events = new Collection<Event>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
    }

    public class IdentityUserLoginConfiguration : EntityTypeConfiguration<IdentityUserLogin>
    {
        public IdentityUserLoginConfiguration()
        {
            HasKey(iul => iul.UserId);
        }
    }

    public class IdentityUserRoleConfiguration : EntityTypeConfiguration<IdentityUserRole>
    {
        public IdentityUserRoleConfiguration()
        {
            HasKey(iur => iur.RoleId);
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("PrimaryConnectionString", throwIfV1Schema: false) { }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<OfferFriendship> OfferFriendships { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Gallery> Galleries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // должно помочь для работы OnModelCreating
            modelBuilder.Configurations.Add(new IdentityUserLoginConfiguration());
            modelBuilder.Configurations.Add(new IdentityUserRoleConfiguration());
            //--------------------------------------

            //configure model with fluent API

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Events)
                .WithMany(e => e.Owners);

            modelBuilder.Entity<Friendship>()
                .HasRequired(f => f.FirstUser)
                .WithMany(u => u.Friendships);

            modelBuilder.Entity<Friendship>()
               .HasRequired(f => f.SecondUser);

            modelBuilder.Entity<OfferFriendship>()
                .HasRequired(f => f.Offer)
                .WithMany(u => u.OfferFriendships);

            modelBuilder.Entity<OfferFriendship>()
                .HasRequired(f => f.Received);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Sender);

            //modelBuilder.Entity<Photo>().
            //    HasRequired(p => p.Sender).
            //    WithMany(u => u.Photos);
        }
    }
}
