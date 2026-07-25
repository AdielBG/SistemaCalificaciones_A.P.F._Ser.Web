using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/profesores")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class ProfesoresController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public ProfesoresController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los profesores
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Recupera todos los profesores almacenados en la base de datos.
            var profesores = await _context.Profesores.ToListAsync();
            // Devuelve los datos en formato JSON con código de estado 200 (OK).
            return Ok(profesores);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un profesor por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var profesor = await _context.Profesores.FindAsync(id);

            if (profesor == null)
            {
                return NotFound(new { mensaje = "No se encontró un profesor con el Id " + id });
            }

            return Ok(profesor);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear un nuevo profesor
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Profesor profesor)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Profesor.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el correo no esté duplicado
            // Se asume que el correo debe ser único para cada profesor,
            // aunque no exista una restricción explícita a nivel de base de datos.
            bool correoExiste = await _context.Profesores
                .AnyAsync(p => p.Correo == profesor.Correo);

            if (correoExiste)
            {
                return Conflict(new { mensaje = "Ya existe un profesor registrado con ese correo." });
            }

            _context.Profesores.Add(profesor);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = profesor.Id }, profesor);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar un profesor existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Profesor profesorActualizado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Profesor.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var profesor = await _context.Profesores.FindAsync(id);

            if (profesor == null)
            {
                return NotFound(new { mensaje = "No se encontró un profesor con el Id " + id });
            }

            // Verificar correo duplicado excluyendo al mismo profesor
            // Se excluye el propio Id del profesor que se está actualizando para
            // no marcar como duplicado su correo actual.
            bool correoRepetido = await _context.Profesores
                .AnyAsync(p => p.Correo == profesorActualizado.Correo && p.Id != id);

            if (correoRepetido)
            {
                return Conflict(new { mensaje = "Ya existe un profesor registrado con ese correo." });
            }

            // Reemplazar todos los campos
            profesor.Nombre = profesorActualizado.Nombre;
            profesor.Apellido = profesorActualizado.Apellido;
            profesor.Correo = profesorActualizado.Correo;
            profesor.Telefono = profesorActualizado.Telefono;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar un profesor
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var profesor = await _context.Profesores.FindAsync(id);

            if (profesor == null)
            {
                return NotFound(new { mensaje = "No se encontró un profesor con el Id " + id });
            }

            // No se puede eliminar si tiene materias asociadas
            // Se evita eliminar un profesor si ya existen materias asignadas a él,
            // para no dejar registros huérfanos o inconsistentes.
            bool tieneMaterias = await _context.Materias
                .AnyAsync(m => m.ProfesorId == id);

            if (tieneMaterias)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el profesor porque tiene materias asociadas." });
            }

            _context.Profesores.Remove(profesor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}