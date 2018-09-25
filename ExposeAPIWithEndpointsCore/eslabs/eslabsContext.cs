using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ExposeAPIWithEndpointsCore.eslabs
{
    public partial class eslabsContext : DbContext
    {
        public eslabsContext()
        {
        }

        public eslabsContext(DbContextOptions<eslabsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblEnbloc> TblEnbloc { get; set; }

        // Unable to generate entity type for table 'test'. Please see the warning messages.
        // Unable to generate entity type for table 'visits'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=35.200.194.132;port=3306;user=root;password=pop@123456;database=eslabs");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblEnbloc>(entity =>
            {
                entity.ToTable("tblEnbloc");

                entity.Property(e => e.Id).HasColumnType("int(250)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(2000)");
            });
        }
    }
}
