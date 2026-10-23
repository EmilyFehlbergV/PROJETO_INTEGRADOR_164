using Microsoft.EntityFrameworkCore;
using VistoPorMim.Models;

namespace VistoPorMim.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Local> Locais { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<LocalRecurso> LocaisRecursos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LocalRecurso>()
                .HasKey(localRecurso => new
                {
                    localRecurso.IdLocal,
                    localRecurso.IdRecurso
                });
        }
    }
}