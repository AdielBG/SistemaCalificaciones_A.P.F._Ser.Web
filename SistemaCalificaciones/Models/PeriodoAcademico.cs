using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models
{
    public class PeriodoAcademico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del período es requerido.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}