using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models
{
    public class Estudiante
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La matrícula es requerida.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "La matrícula debe tener entre 3 y 20 caracteres.")]
        public string Matricula { get; set; } = string.Empty;

        // Relaciones con otras tablas
        public int ProgramaAcademicoId { get; set; }
        public int SeccionId { get; set; }
        public int EstadoAcademicoId { get; set; }
    }
}