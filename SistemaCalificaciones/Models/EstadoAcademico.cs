using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models
{
    public class EstadoAcademico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del estado es requerido.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "La descripción no puede superar 150 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}