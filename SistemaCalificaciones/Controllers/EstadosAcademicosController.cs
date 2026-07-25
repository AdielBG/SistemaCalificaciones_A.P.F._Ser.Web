using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/estados-academicos")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class EstadosAcademicosController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public EstadosAcademicosController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los estados académicos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Recupera todos los estados académicos almacenados en la base de datos.
            var estados = await _context.EstadosAcademicos.ToListAsync();
            // Devuelve los datos en formato JSON con código de estado 200 (OK).
            return Ok(estados);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un estado académico por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var estado = await _context.EstadosAcademicos.FindAsync(id);

            if (estado == null)
            {
                return NotFound(new { mensaje = "No se encontró un estado académico con el Id " + id });
            }

            return Ok(estado);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear un nuevo estado académico
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] EstadoAcademico estado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo EstadoAcademico.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.EstadosAcademicos.Add(estado);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = estado.Id }, estado);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar un estado académico existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] EstadoAcademico estadoActualizado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo EstadoAcademico.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estado = await _context.EstadosAcademicos.FindAsync(id);

            if (estado == null)
            {
                return NotFound(new { mensaje = "No se encontró un estado académico con el Id " + id });
            }

            // Reemplazar los campos editables
            estado.Nombre = estadoActualizado.Nombre;
            estado.Descripcion = estadoActualizado.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar un estado académico
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var estado = await _context.EstadosAcademicos.FindAsync(id);

            if (estado == null)
            {
                return NotFound(new { mensaje = "No se encontró un estado académico con el Id " + id });
            }

            // Verificar que no tenga estudiantes asociados
            // Se evita eliminar un estado académico si ya existen estudiantes
            // que lo tienen asignado, para no dejar registros huérfanos o inconsistentes.
            bool tieneEstudiantes = await _context.Estudiantes
                .AnyAsync(e => e.EstadoAcademicoId == id);

            if (tieneEstudiantes)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el estado porque tiene estudiantes asociados." });
            }

            _context.EstadosAcademicos.Remove(estado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}