using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/estudiantes")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class EstudiantesController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public EstudiantesController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los estudiantes, incluyendo los nombres de sus relaciones
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Se devuelve información enriquecida con los nombres de las relaciones
            // Se usan subconsultas correlacionadas
            // dentro del Select para resolver el nombre del programa académico,
            // la sección y el estado académico, evitando el problema N+1.
            var estudiantes = await _context.Estudiantes
                .Select(e => new
                {
                    e.Id,
                    e.Nombre,
                    e.Apellido,
                    e.Correo,
                    e.Matricula,
                    e.ProgramaAcademicoId,
                    NombrePrograma = _context.ProgramasAcademicos
                        .Where(p => p.Id == e.ProgramaAcademicoId)
                        .Select(p => p.Nombre)
                        .FirstOrDefault(),
                    e.SeccionId,
                    NombreSeccion = _context.Secciones
                        .Where(s => s.Id == e.SeccionId)
                        .Select(s => s.Nombre)
                        .FirstOrDefault(),
                    e.EstadoAcademicoId,
                    NombreEstado = _context.EstadosAcademicos
                        .Where(ea => ea.Id == e.EstadoAcademicoId)
                        .Select(ea => ea.Nombre)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(estudiantes);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un estudiante por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);

            if (estudiante == null)
            {
                return NotFound(new { mensaje = "No se encontró un estudiante con el Id " + id });
            }

            return Ok(estudiante);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear un nuevo estudiante
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Estudiante estudiante)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Estudiante.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar matrícula duplicada
            // La matrícula es el identificador académico único de cada estudiante.
            bool matriculaExiste = await _context.Estudiantes
                .AnyAsync(e => e.Matricula == estudiante.Matricula);

            if (matriculaExiste)
            {
                return Conflict(new { mensaje = "Ya existe un estudiante con esa matrícula." });
            }

            // Verificar correo duplicado
            bool correoExiste = await _context.Estudiantes
                .AnyAsync(e => e.Correo == estudiante.Correo);

            if (correoExiste)
            {
                return Conflict(new { mensaje = "Ya existe un estudiante con ese correo." });
            }

            // Verificar que el programa académico exista
            // Se validan las tres llaves foráneas del estudiante antes de crearlo
            var programaExiste = await _context.ProgramasAcademicos
                .FindAsync(estudiante.ProgramaAcademicoId);

            if (programaExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un programa académico con el Id " + estudiante.ProgramaAcademicoId });
            }

            // Verificar que la sección exista
            var seccionExiste = await _context.Secciones
                .FindAsync(estudiante.SeccionId);

            if (seccionExiste == null)
            {
                return BadRequest(new { mensaje = "No existe una sección con el Id " + estudiante.SeccionId });
            }

            // Verificar que el estado académico exista
            var estadoExiste = await _context.EstadosAcademicos
                .FindAsync(estudiante.EstadoAcademicoId);

            if (estadoExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un estado académico con el Id " + estudiante.EstadoAcademicoId });
            }

            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = estudiante.Id }, estudiante);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar un estudiante existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Estudiante estudianteActualizado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Estudiante.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estudiante = await _context.Estudiantes.FindAsync(id);

            if (estudiante == null)
            {
                return NotFound(new { mensaje = "No se encontró un estudiante con el Id " + id });
            }

            // Verificar matrícula duplicada excluyendo al mismo estudiante
            bool matriculaRepetida = await _context.Estudiantes
                .AnyAsync(e => e.Matricula == estudianteActualizado.Matricula && e.Id != id);

            if (matriculaRepetida)
            {
                return Conflict(new { mensaje = "Ya existe un estudiante con esa matrícula." });
            }

            // Verificar correo duplicado excluyendo al mismo estudiante
            bool correoRepetido = await _context.Estudiantes
                .AnyAsync(e => e.Correo == estudianteActualizado.Correo && e.Id != id);

            if (correoRepetido)
            {
                return Conflict(new { mensaje = "Ya existe un estudiante con ese correo." });
            }

            // Verificar que las relaciones existan
            // Misma validación de las tres llaves foráneas que en Crear, ya que el
            // estudiante podría estar cambiando de programa, sección o estado académico.
            var programaExiste = await _context.ProgramasAcademicos
                .FindAsync(estudianteActualizado.ProgramaAcademicoId);

            if (programaExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un programa académico con el Id " + estudianteActualizado.ProgramaAcademicoId });
            }

            var seccionExiste = await _context.Secciones
                .FindAsync(estudianteActualizado.SeccionId);

            if (seccionExiste == null)
            {
                return BadRequest(new { mensaje = "No existe una sección con el Id " + estudianteActualizado.SeccionId });
            }

            var estadoExiste = await _context.EstadosAcademicos
                .FindAsync(estudianteActualizado.EstadoAcademicoId);

            if (estadoExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un estado académico con el Id " + estudianteActualizado.EstadoAcademicoId });
            }

            // Reemplazar todos los campos
            estudiante.Nombre = estudianteActualizado.Nombre;
            estudiante.Apellido = estudianteActualizado.Apellido;
            estudiante.Correo = estudianteActualizado.Correo;
            estudiante.Matricula = estudianteActualizado.Matricula;
            estudiante.ProgramaAcademicoId = estudianteActualizado.ProgramaAcademicoId;
            estudiante.SeccionId = estudianteActualizado.SeccionId;
            estudiante.EstadoAcademicoId = estudianteActualizado.EstadoAcademicoId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar un estudiante
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);

            if (estudiante == null)
            {
                return NotFound(new { mensaje = "No se encontró un estudiante con el Id " + id });
            }

            // Verificar que no tenga calificaciones asociadas
            // Se evita eliminar un estudiante si ya existen calificaciones registradas
            // a su nombre, para no dejar registros huérfanos o inconsistentes.
            bool tieneCalificaciones = await _context.Calificaciones
                .AnyAsync(c => c.EstudianteId == id);

            if (tieneCalificaciones)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el estudiante porque tiene calificaciones registradas." });
            }

            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}