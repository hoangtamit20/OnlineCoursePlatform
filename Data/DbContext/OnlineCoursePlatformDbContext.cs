using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCoursePlatform.Data.Entities;
using OnlineCoursePlatform.Helpers.UrlHelpers;

namespace OnlineCoursePlatform.Data.DbContext
{
    public class OnlineCoursePlatformDbContext : IdentityDbContext<AppUser>
    {
        
        public OnlineCoursePlatformDbContext(DbContextOptions<OnlineCoursePlatformDbContext> options)
            : base(options)
        {
            
        }

        #region dbset

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        public DbSet<UrlHelperEntity> UrlHelperEntities {get; set; }

        #endregion

        #pragma warning disable CS8618 // Required by Entity Framework
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<AppUser>(b =>
            {
                b.HasIndex(u => u.Email).IsUnique();
                b.HasIndex(u => u.UserName).IsUnique();
            });

            builder.Entity<AppUser>().Property(b => b.Email).IsRequired();
            builder.Entity<AppUser>().Property(b => b.UserName).IsRequired();


            builder.Entity<UserRefreshToken>(usr => 
            {
                usr.HasIndex(usr => usr.AccessToken).IsUnique();
                usr.HasIndex(usr => usr.RefreshToken).IsUnique();
            });

            foreach (var entityType in builder.Model.GetEntityTypes())
                if (entityType.GetTableName()!.StartsWith("AspNet"))
                    entityType.SetTableName(entityType.GetTableName()!.Substring(6));
        }
    }
}