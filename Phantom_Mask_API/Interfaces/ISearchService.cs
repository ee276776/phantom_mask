using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAsync(string query, string type = "all", int limit = 50);
        Task<List<PharmacyDto>> SearchPharmaciesAsync(string query, int limit = 50);
        Task<List<MaskDto>> SearchMasksAsync(string query, int limit = 50);
        Task<List<RelevanceResultDto>> SearchByRelavanceAsync(string query);

    }
}
