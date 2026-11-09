using Microsoft.EntityFrameworkCore;
using RaceDayApp_POE.Models;
using Results = RaceDayApp_POE.Models.Results;

namespace RaceDayApp_POE.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) 
        { 
        
        }

        public DbSet<Users> User { get; set; }

        public DbSet<Categories> Category { get; set; }
        
        public DbSet<Enrollments> Enrollment { get; set; }

        public DbSet<Events> Event { get; set; }

        public DbSet<Results> Result { get; set; }

        public DbSet<UserRoles> UserRole { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //UserRole -> Users
            modelBuilder.Entity<Users>()
                .HasOne(u => u.userRole)
                .WithMany(r => r.User)
                .HasForeignKey(u => u.roleID)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> Event (Organizer)
            modelBuilder.Entity<Events>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.Event)
                .HasForeignKey(e => e.organiserID)
                .OnDelete(DeleteBehavior.NoAction);

            // Event -> Category
            modelBuilder.Entity<Categories>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Category)
                .HasForeignKey(c => c.eventID)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> Enrollment (Participant)
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.Participant)
                .WithMany(u => u.Enrollment)
                .HasForeignKey(e => e.participantID)
                .OnDelete(DeleteBehavior.NoAction);

            // Event -> Enrollment
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrollment)
                .HasForeignKey(e => e.eventID)
                .OnDelete(DeleteBehavior.NoAction);

            // Category -> Enrollment
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrollment)
                .HasForeignKey(e => e.categoryID)
                .OnDelete(DeleteBehavior.NoAction);

            // Enrollment -> Result
            modelBuilder.Entity<Results>()
                .HasOne(r => r.Enrollment)
                .WithOne(e => e.Result)
                .HasForeignKey<Results>(r => r.enrollmentID)
                .OnDelete(DeleteBehavior.NoAction);

        }//end of override: OnModelCreating

    }//end of class: RaceDayDbContext

}//end of namespace
