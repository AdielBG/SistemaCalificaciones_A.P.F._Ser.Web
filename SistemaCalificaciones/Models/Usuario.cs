namespace SistemaCalificaciones.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string HashContrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}