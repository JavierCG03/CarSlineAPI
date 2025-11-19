using Microsoft.EntityFrameworkCore;
using CarSlineAPI.Models;

namespace CarSlineAPI.Data
{
    /// <summary>
    /// Contexto de base de datos para CarSline
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Tablas de la base de datos
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CONFIGURACIÓN DE USUARIO
            // ============================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                // Índice único en NombreUsuario
                entity.HasIndex(e => e.NombreUsuario)
                    .IsUnique()
                    .HasDatabaseName("idx_nombre_usuario");

                // Índice en RolId
                entity.HasIndex(e => e.RolId)
                    .HasDatabaseName("idx_rol");

                // Índice en Activo
                entity.HasIndex(e => e.Activo)
                    .HasDatabaseName("idx_activo");

                // Valor por defecto para FechaCreacion
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Valor por defecto para Activo
                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                // Relación con Rol (muchos usuarios tienen un rol)
                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.RolId)
                    .OnDelete(DeleteBehavior.Restrict); // No eliminar rol si tiene usuarios

                // Auto-referencia para CreadoPor (un usuario puede ser creado por otro)
                entity.HasOne(e => e.CreadoPor)
                    .WithMany()
                    .HasForeignKey(e => e.CreadoPorId)
                    .OnDelete(DeleteBehavior.SetNull); // Si se elimina el creador, se pone NULL
            });

            // ============================================
            // CONFIGURACIÓN DE ROL
            // ============================================
            modelBuilder.Entity<Rol>(entity =>
            {
                // Índice único en NombreRol
                entity.HasIndex(e => e.NombreRol)
                    .IsUnique()
                    .HasDatabaseName("idx_nombre_rol");

                // Valor por defecto para FechaCreacion
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}