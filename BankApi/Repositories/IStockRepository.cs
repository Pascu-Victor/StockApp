﻿using BankApi.Models;

namespace BankApi.Repositories
{
    public interface IStockRepository
    {
        Task<Stock> CreateAsync(Stock stock);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Stock>> GetAllAsync();
        Task<Stock?> GetByIdAsync(int id);
        Task<Stock?> UpdateAsync(int id, Stock updatedStock);
        Task<List<Stock>> UserStocksAsync(string cnp);
    }
}