namespace BankApi.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApi.Data;
    using BankApi.Models;
    using Microsoft.EntityFrameworkCore;

    public class HomepageStocksRepository : IHomepageStocksRepository
    {
        private readonly ApiDbContext _context;

        public HomepageStocksRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomepageStock>> GetAllStocksAsync()
        {
            return await _context.HomepageStocks.ToListAsync();
        }

        public async Task<HomepageStock?> GetStockByIdAsync(int id)
        {
            return await _context.HomepageStocks.FindAsync(id);
        }

        public async Task AddStockAsync(HomepageStock stock)
        {
            _context.HomepageStocks.Add(stock);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStockAsync(HomepageStock stock)
        {
            _context.HomepageStocks.Update(stock);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStockAsync(int id)
        {
            var stock = await _context.HomepageStocks.FindAsync(id);
            if (stock != null)
            {
                _context.HomepageStocks.Remove(stock);
                await _context.SaveChangesAsync();
            }
        }
    }
}
