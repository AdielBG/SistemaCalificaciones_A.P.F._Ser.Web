using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models
{
    public class Seccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sección es requerido.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 200, ErrorMessage = "La capacidad debe estar entre 1 y 200 estudiantes.")]
        public int Capacidad { get; set; }
    }
}