using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Models.Auth;
using SistemaCalificaciones.Seguridad;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        // Servicio encargado de generar y configurar los tokens JWT.
        private readonly JwtService _jwt;

        // Utilidad de ASP.NET Identity para encriptar y verificar contraseñas de forma segura.
        private readonly PasswordHasher<Usuario> _hasher = new();

        public AuthController(AppDbContext context, JwtService jwt)
        {
            // Inyección de dependencias del contexto de base de datos y del servicio JWT.
            _context = context;
            _jwt = jwt;
        }


        // ENDPOINT 1: Registrar un nuevo usuario
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en RegistroRequest.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el nombre de usuario no exista
            bool nombreUsuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == request.NombreUsuario);

            if (nombreUsuarioExiste)
            {
                return Conflict(new { mensaje = "El nombre de usuario ya está en uso." });
            }

            // Verificar que el correo no exista
            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == request.Correo);

            if (correoExiste)
            {
                return Conflict(new { mensaje = "El correo ya está registrado." });
            }

            // Crear el nuevo usuario
            // Se asigna el rol "Usuario" por defecto y la fecha de registro actual en UTC.
            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                NombreUsuario = request.NombreUsuario,
                Correo = request.Correo,
                Rol = "Usuario",
                FechaRegistro = DateTime.UtcNow
            };

            // Encriptar la contraseña antes de guardarla
            // Nunca se almacena la contraseña en texto plano; se guarda únicamente su hash.
            usuario.HashContrasena = _hasher.HashPassword(usuario, request.Contrasena);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Generar el token y devolverlo
            // Tras el registro exitoso, se autentica al usuario automáticamente
            // generando su token JWT junto con la fecha de expiración.
            var (token, expira) = _jwt.GenerarToken(usuario);

            return Ok(new AuthResponse
            {
                Token = token,
                Expira = expira,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            });
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Iniciar sesión (login) y obtener un token JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en LoginRequest.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar el usuario por nombre de usuario
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario);

            // Si no existe el usuario, devolvemos el mismo mensaje que si la contraseña es incorrecta.
            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas." });
            }

            // Verificar la contraseña comparando con el hash guardado
            // PasswordHasher compara la contraseña ingresada contra el hash almacenado
            // sin necesidad de desencriptarlo.
            var resultado = _hasher.VerifyHashedPassword(
                usuario, usuario.HashContrasena, request.Contrasena);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas." });
            }

            // Generar el token
            // Si las credenciales son correctas, se genera el token JWT que el cliente
            // deberá enviar en los siguientes requests para autenticarse.
            var (token, expira) = _jwt.GenerarToken(usuario);

            return Ok(new AuthResponse
            {
                Token = token,
                Expira = expira,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            });
        }
    }
}