using Microsoft.EntityFrameworkCore;
using FaceRecognitionWebsite.Models;

namespace FaceRecognitionWebsite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<FaceImage> FaceImages { get; set; }

    }
}