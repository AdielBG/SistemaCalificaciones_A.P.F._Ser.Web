using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/programas-academicos")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class ProgramasAcademicosController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public ProgramasAcademicosController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los programas académicos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Recupera todos los programas académicos almacenados en la base de datos.
            var programas = await _context.ProgramasAcademicos.ToListAsync();
            // Devuelve los datos en formato JSON con código de estado 200 (OK).
            return Ok(programas);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un programa académico por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var programa = await _context.ProgramasAcademicos.FindAsync(id);

            if (programa == null)
            {
                return NotFound(new { mensaje = "No se encontró un programa académico con el Id " + id });
            }

            return Ok(programa);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear un nuevo programa académico
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ProgramaAcademico programa)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo ProgramaAcademico.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ProgramasAcademicos.Add(programa);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = programa.Id }, programa);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar un programa académico existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProgramaAcademico programaActualizado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo ProgramaAcademico.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var programa = await _context.ProgramasAcademicos.FindAsync(id);

            if (programa == null)
            {
                return NotFound(new { mensaje = "No se encontró un programa académico con el Id " + id });
            }

            // Reemplazar los campos editables
            programa.Nombre = programaActualizado.Nombre;
            programa.Descripcion = programaActualizado.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar un programa académico
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var programa = await _context.ProgramasAcademicos.FindAsync(id);

            if (programa == null)
            {
                return NotFound(new { mensaje = "No se encontró un programa académico con el Id " + id });
            }

            // Verificar que no tenga estudiantes asociados
            // Se evita eliminar un programa académico si existen estudiantes
            // relacionados a él, para no dejar registros huérfanos o inconsistentes.
            bool tieneEstudiantes = await _context.Estudiantes
                .AnyAsync(e => e.ProgramaAcademicoId == id);

            if (tieneEstudiantes)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el programa porque tiene estudiantes asociados." });
            }

            _context.ProgramasAcademicos.Remove(programa);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}