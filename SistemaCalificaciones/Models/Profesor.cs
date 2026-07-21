using System.ComponentModel.DataAnnotations;

namespace SistemaCalificaciones.Models
{
    public class Profesor
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

        [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
        public string Telefono { get; set; } = string.Empty;
    }
}