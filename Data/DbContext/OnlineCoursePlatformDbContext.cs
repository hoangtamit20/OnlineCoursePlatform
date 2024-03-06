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

        public DbSet<UrlHelperEntity> UrlHelperEntities { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<UserCourseInteraction> UserCourseInteractions { get; set; }

        public DbSet<CourseType> CourseTypes { get; set; }

        public DbSet<CourseTopic> CourseTopics { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSubtitle> CourseSubtitles { get; set; }
        public DbSet<CourseUrlSteaming> CourseUrlSteamings { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonSubtitle> LessonSubtitles { get; set; }
        public DbSet<LessonUrlStreaming> LessonUrlStreamings { get; set; }
        public DbSet<Cart> Carts { get; set; }

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

            builder.Entity<UserCourseInteraction>()
                .HasIndex(uci => new { uci.UserId, uci.CourseId })
                .IsUnique();

            builder.Entity<UserCourseInteraction>()
                .HasIndex(uci => new { uci.CourseId, uci.IpAddress })
                .IsUnique();

            builder.Entity<CourseType>(ct =>
            {
                ct.HasIndex(ct => ct.Name).IsUnique();
            });

            builder.Entity<CourseTopic>(cp =>
            {
                cp.HasIndex(cp => cp.Name).IsUnique();
            });

            builder.Entity<Lesson>(ls =>
            {
                ls.HasIndex(ls => ls.Name).IsUnique();
            });

            builder.Entity<Course>(c =>
            {
                c.HasIndex(c => c.Name).IsUnique();
            });

            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Cart>()
                .HasOne(c => c.Course)
                .WithMany(co => co.Carts)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Cart>()
                .HasIndex(c => new { c.UserId, c.CourseId })
                .IsUnique();


            foreach (var entityType in builder.Model.GetEntityTypes())
                if (entityType.GetTableName()!.StartsWith("AspNet"))
                    entityType.SetTableName(entityType.GetTableName()!.Substring(6));
        }
    }
}