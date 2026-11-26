using Microsoft.EntityFrameworkCore;
using ClinicSystem.Models;

namespace ClinicSystem.Data
{
    // The DbContext is the bridge between our C# code and the MySQL database.
    // Each DbSet<...> below represents one table.
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccount> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<ClinicStaff> StaffMembers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ConsultationRecord> ConsultationRecords { get; set; }

        // Here we tell Entity Framework Core the exact table names and the rules
        // between the tables (primary keys, foreign keys, and delete behavior).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names (they must match the SQL script)
            modelBuilder.Entity<UserAccount>().ToTable("Users");
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Doctor>().ToTable("Doctors");
            modelBuilder.Entity<ClinicStaff>().ToTable("ClinicStaff");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<ConsultationRecord>().ToTable("ConsultationRecords");

            // One account per email address (BR-9)
            modelBuilder.Entity<UserAccount>().HasIndex(u => u.Email).IsUnique();

            // Patient, Doctor, and ClinicStaff each belong to one UserAccount
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ClinicStaff>()
                .HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One patient / one doctor has many appointments (BR-1).
            // Restrict = the database refuses to delete a patient or doctor who still has appointments (BR-8).
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor).WithMany(d => d.Appointments).HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // One appointment has at most one consultation record
            modelBuilder.Entity<ConsultationRecord>()
                .HasOne(c => c.Appointment).WithOne(a => a.ConsultationRecord)
                .HasForeignKey<ConsultationRecord>(c => c.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
