using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories
{
    /// <summary>Resultado agregado de GetTopRatedAsync: no es una entidad, solo un valor de consulta.</summary>
    public class GameRatingSummary
    {
        public Game Game { get; set; } = null!;
        public double AverageRate { get; set; }
        public int RatingsCount { get; set; }
    }
}
