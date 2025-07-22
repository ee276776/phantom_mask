using PhantomMaskAPI.Models.Entities;

namespace PhantomMaskAPI.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserByNameAsync(string name);
        Task<List<User>> GetUsersWithHighBalanceAsync(decimal minimumBalance);
        Task<List<User>> GetTopSpendersAsync(DateTime startDate, DateTime endDate, int topN);
        Task UpdateUserBalanceAsync(string userName, decimal newBalance);
        Task UpdateUserBalanceByIdAsync(int userId, decimal newBalance);

        Task<bool> UserExistsAsync(string userName);
        Task<bool> UserExistsAsync(int userId);

    }
}
