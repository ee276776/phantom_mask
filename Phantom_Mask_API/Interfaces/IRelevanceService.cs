using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Interfaces
{
    public interface IRelevanceService
    {
        double CalculateRelevanceScoreInternal(string query, RelevanceDto item);
    }
}
