using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCalificaciones.Models
{
    public class Calificacion
    {
        public int Id { get; set; }

        // Relaciones con otras tablas
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un EstudianteId válido.")]
        public int EstudianteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un MateriaId válido.")]
        public int MateriaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un PeriodoAcademicoId válido.")]
        public int PeriodoAcademicoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un TipoEvaluacionId válido.")]
        public int TipoEvaluacionId { get; set; }

        // Las cuatro calificaciones parciales
        [Range(0, 100, ErrorMessage = "La Calificación 1 debe estar entre 0 y 100.")]
        public decimal Calificacion1 { get; set; }

        [Range(0, 100, ErrorMessage = "La Calificación 2 debe estar entre 0 y 100.")]
        public decimal Calificacion2 { get; set; }

        [Range(0, 100, ErrorMessage = "La Calificación 3 debe estar entre 0 y 100.")]
        public decimal Calificacion3 { get; set; }

        [Range(0, 100, ErrorMessage = "La Calificación 4 debe estar entre 0 y 100.")]
        public decimal Calificacion4 { get; set; }

        [Range(0, 100, ErrorMessage = "El Examen debe estar entre 0 y 100.")]
        public decimal Examen { get; set; }

        // Campos calculados automáticamente - no los envía el cliente
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalCalificacion { get; private set; }

        public string Clasificacion { get; private set; } = string.Empty;
        public string Estado { get; private set; } = string.Empty;

        // Método que calcula automáticamente los campos derivados
        public void CalcularResultados()
        {
            // Fórmula: 70% del promedio de las 4 notas + 30% del examen
            decimal promedioParciales = (Calificacion1 + Calificacion2 + Calificacion3 + Calificacion4) / 4;
            TotalCalificacion = Math.Round((promedioParciales * 0.70m) + (Examen * 0.30m), 2);

            // Clasificación según el total
            if (TotalCalificacion >= 90)
                Clasificacion = "A - Excelente";
            else if (TotalCalificacion >= 80)
                Clasificacion = "B - Bueno";
            else if (TotalCalificacion >= 70)
                Clasificacion = "C - Aceptable";
            else
                Clasificacion = "F - Insuficiente";

            // Estado según el total
            if (TotalCalificacion >= 70)
                Estado = "Aprobado";
            else
                Estado = "Reprobado";
        }
    }
}