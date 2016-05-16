using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebApplication.Models
{
    // Чтобы добавить данные профиля для пользователя, можно добавить дополнительные свойства в класс ApplicationUser. 
    //Дополнительные сведения см. по адресу: http://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public int PostsCount
        { get; set; }

        public int SubscribersCount
        { get; set; }

        public int SubscriptionCount
        { get; set; }

        public DateTime? Age
        { get; set; }

        public string Gender
        { get; set; }

        public string Name
        { get; set; }

        public string Surname
        { get; set; }

        public string Country
        { get; set; }

        public string City
        { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            userIdentity.AddClaim(new Claim(ClaimTypes.Gender, this.Gender));
            userIdentity.AddClaim(new Claim("age", this.Age.ToString()));
            return userIdentity;
        }
    }

    public class Post
    {
        [Key]
        public virtual int Id
        { get; set; }

        public virtual string Title
        { get; set; }

        public virtual string ShortDescription
        { get; set; }

        public virtual string Description
        { get; set; }

        public virtual string Meta
        { get; set; }

        public virtual string UrlSlug
        { get; set; }

        public virtual bool Published
        { get; set; }

        public virtual DateTime PostedOn
        { get; set; }

        public virtual DateTime? Modified
        { get; set; }

        public virtual IList<Tag> Tags
        { get; set; }

        public int LikeCount
        { get; set; }

        public int CommentsCount
        { get; set; }

        public string Editor
        { get; set; }

        public string Lens
        { get; set; }
    }

    public class Tag
    {
        [Key]
        public virtual int Id
        { get; set; }

        public virtual string Name
        { get; set; }

        public virtual string UrlSlug
        { get; set; }

        public virtual string Description
        { get; set; }

        public virtual IList<Post> Posts
        { get; set; }
    }

    public partial class Image
    {
        [Key]
        public int Id
        { get; set; }

        public string ImageName
        { get; set; }

        public string ImagePath
        { get; set; }

        public string Description
        { get; set; }

        public int Size
        { get; set; }

        public byte[] ImageData
        { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Image> Images { get; set; }
        public ApplicationDbContext() : base("PrimaryConnectionString", throwIfV1Schema: false) { }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}