using Microsoft.EntityFrameworkCore;

namespace FhirApi.Models
{
    public class FhirDbContext : DbContext
    {
        public FhirDbContext(DbContextOptions<FhirDbContext> options) : base(options) { }

        public DbSet<PatientResource> Patients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientResource>().HasKey(p => p.Id);
        }
    }

    public class PatientResource
    {
        public string Id { get; set; } // Primary key for the resource
        public string ResourceJson { get; set; } // Store FHIR resource as JSON string
    }
}
