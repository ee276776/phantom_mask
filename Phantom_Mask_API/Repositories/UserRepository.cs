using Microsoft.EntityFrameworkCore;
using PhantomMaskAPI.Data;
using PhantomMaskAPI.Models.Entities;
using PhantomMaskAPI.Interfaces;

namespace PhantomMaskAPI.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(PhantomMaskContext context) : base(context)
        {
        }

        public async Task<User?> GetUserByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task<List<User>> GetTopSpendersAsync(DateTime startDate, DateTime endDate, int topN)
        {
            var topSpenders = await _context.Purchases
                .Where(p => p.TransactionDateTime >= startDate && p.TransactionDateTime <= endDate)
                .GroupBy(p => p.UserName)
                .Select(g => new
                {
                    UserName = g.Key,
                    TotalSpent = g.Sum(p => p.TransactionAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(topN)
                .ToListAsync();

            var userNames = topSpenders.Select(x => x.UserName).ToList();
            var users = await _dbSet
                .Where(u => userNames.Contains(u.Name))
                .ToListAsync();

            // 按照消費金額排序返回
            return users
                .OrderByDescending(u => topSpenders.First(ts => ts.UserName == u.Name).TotalSpent)
                .ToList();
        }

        public async Task<List<User>> GetUsersWithHighBalanceAsync(decimal minBalance)
        {
            return await _dbSet
                .Where(u => u.CashBalance >= minBalance)
                .ToListAsync();
        }

        public async Task UpdateUserBalanceAsync(string userName, decimal newBalance)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Name == userName);
            if (user != null)
            {
                user.CashBalance = newBalance;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _dbSet.AnyAsync(u => u.Name == userName);
        }

        public async Task UpdateCashBalanceAsync(int userId, decimal newBalance)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.CashBalance = newBalance;
                await _context.SaveChangesAsync();
            }
        }
    }
}
