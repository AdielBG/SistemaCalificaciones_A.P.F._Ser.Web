using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Seguridad
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        // Recibe la configuración de la aplicación mediante
        // inyección de dependencias para acceder a los valores
        // definidos en appsettings.json (como la clave JWT).
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime Expira) GenerarToken(Usuario usuario)
        {
            // Obtiene la sección "Jwt" del archivo appsettings.json,
            // donde se encuentran la clave secreta, el emisor,
            // la audiencia y el tiempo de expiración del token.
            var jwt = _configuration.GetSection("Jwt");

            // Convierte la clave secreta en un arreglo de bytes y crea
            // la llave criptográfica utilizada para firmar digitalmente
            // el token con el algoritmo HMAC SHA-256.
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            // Calcula la fecha y hora de expiración del token tomando
            // como referencia la hora UTC y sumando los minutos
            // configurados en el archivo de configuración.
            var expira = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiraEnMinutos"]!));

            // Define los Claims (información que viajará dentro del JWT).
            // Estos datos permiten identificar al usuario y conocer
            // su rol sin necesidad de consultar la base de datos
            // en cada solicitud autenticada.
            var claims = new List<Claim>
            {
                // Identificador único del usuario.
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),

                // Nombre de usuario utilizado para identificarlo.
                new(JwtRegisteredClaimNames.UniqueName, usuario.NombreUsuario),

                // Dirección de correo electrónico del usuario.
                new(JwtRegisteredClaimNames.Email, usuario.Correo),

                // Rol del usuario para aplicar autorización
                // basada en permisos.
                new("role", usuario.Rol),

                // Identificador único del token para evitar
                // duplicados y facilitar su rastreo.
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Crea el objeto JWT incorporando el emisor, la audiencia,
            // los claims, la fecha de expiración y la firma digital.
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: credenciales);

            // Convierte el objeto JWT en una cadena de texto
            // que será enviada al cliente junto con la fecha
            // en la que dejará de ser válido.
            return (new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}