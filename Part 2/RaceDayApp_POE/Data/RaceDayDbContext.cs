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

    }
}
