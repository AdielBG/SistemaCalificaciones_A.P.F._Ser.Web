using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models
{
    public class ProgramaAcademico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del programa es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede superar 250 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
