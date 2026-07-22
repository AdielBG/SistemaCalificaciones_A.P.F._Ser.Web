using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor que recibe las opciones de configuración del contexto
        // (cadena de conexión, proveedor de base de datos, etc.) mediante
        // inyección de dependencias.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Cada DbSet representa una tabla dentro de la base de datos.

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<ProgramaAcademico> ProgramasAcademicos { get; set; }
        public DbSet<PeriodoAcademico> PeriodosAcademicos { get; set; }
        public DbSet<Seccion> Secciones { get; set; }
        public DbSet<TipoEvaluacion> TiposEvaluacion { get; set; }
        public DbSet<EstadoAcademico> EstadosAcademicos { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura el modelo de la base de datos mediante la API Fluent.
            // Aquí se definen restricciones, índices, relaciones y propiedades
            // que Entity Framework aplicará al crear o actualizar la base de datos.

            // Crea un índice único para el nombre de usuario,
            // evitando que existan dos usuarios con el mismo nombre.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            // Crea un índice único para el correo electrónico,
            // garantizando que cada usuario tenga un correo diferente.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // Impide que dos estudiantes compartan el mismo número de matrícula.
            modelBuilder.Entity<Estudiante>()
                .HasIndex(e => e.Matricula)
                .IsUnique();

            // Garantiza que el código de cada materia sea único.
            modelBuilder.Entity<Materia>()
                .HasIndex(m => m.Codigo)
                .IsUnique();

            // Define la precisión y escala de los campos decimales de las
            // calificaciones. En este caso, hasta 5 dígitos en total,
            // con 2 decimales (ejemplo: 100.00).
            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Calificacion1)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Calificacion2)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Calificacion3)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Calificacion4)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Calificacion>()
                .Property(c => c.Examen)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Calificacion>()
                .Property(c => c.TotalCalificacion)
                .HasPrecision(5, 2);

            // Configura la relación entre Calificación y Estudiante.
            // Cada calificación pertenece a un estudiante y no se permitirá
            // eliminar un estudiante si existen calificaciones asociadas.
            modelBuilder.Entity<Calificacion>()
                .HasOne<Estudiante>()
                .WithMany()
                .HasForeignKey(c => c.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configura la relación entre Calificación y Materia,
            // evitando eliminar una materia con calificaciones registradas.
            modelBuilder.Entity<Calificacion>()
                .HasOne<Materia>()
                .WithMany()
                .HasForeignKey(c => c.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciona la calificación con el período académico correspondiente
            // e impide eliminar un período que tenga registros asociados.
            modelBuilder.Entity<Calificacion>()
                .HasOne<PeriodoAcademico>()
                .WithMany()
                .HasForeignKey(c => c.PeriodoAcademicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciona la calificación con un tipo de evaluación
            // (parcial, final, práctica, etc.) y restringe su eliminación
            // si existen calificaciones vinculadas.
            modelBuilder.Entity<Calificacion>()
                .HasOne<TipoEvaluacion>()
                .WithMany()
                .HasForeignKey(c => c.TipoEvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciona cada estudiante con un programa académico
            // y evita eliminar el programa mientras tenga estudiantes asignados.
            modelBuilder.Entity<Estudiante>()
                .HasOne<ProgramaAcademico>()
                .WithMany()
                .HasForeignKey(e => e.ProgramaAcademicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciona cada estudiante con una sección y restringe
            // la eliminación de la sección si tiene estudiantes asignados.
            modelBuilder.Entity<Estudiante>()
                .HasOne<Seccion>()
                .WithMany()
                .HasForeignKey(e => e.SeccionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciona cada estudiante con su estado académico
            // (activo, retirado, graduado, etc.) e impide eliminar
            // un estado que esté siendo utilizado.
            modelBuilder.Entity<Estudiante>()
                .HasOne<EstadoAcademico>()
                .WithMany()
                .HasForeignKey(e => e.EstadoAcademicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configura la relación entre Materia y Profesor.
            // Cada materia tiene un profesor asignado y no podrá eliminarse
            // un profesor mientras existan materias asociadas.
            modelBuilder.Entity<Materia>()
                .HasOne<Profesor>()
                .WithMany()
                .HasForeignKey(m => m.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}