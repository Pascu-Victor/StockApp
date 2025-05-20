﻿namespace BankApi.Services
{
    using BankApi.Repositories;
    using Common.Models;
    using Common.Services;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class StockService(IStockRepository stockRepository, IHomepageStockRepository homepageStocksRepository) : IStockService
    {
        private readonly IStockRepository stockRepository = stockRepository;
        private readonly IHomepageStockRepository homepageStocksRepo = homepageStocksRepository;

        /// <inheritdoc/>
        public async Task<Stock> CreateStockAsync(Stock stock)
        {
            return await stockRepository.CreateAsync(stock);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteStockAsync(int id)
        {
            return await stockRepository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Stock>> GetAllStocksAsync()
        {
            return await stockRepository.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<Stock> GetStockByIdAsync(int id)
        {
            return await stockRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Stock> UpdateStockAsync(int id, Stock updatedStock)
        {
            return await stockRepository.UpdateAsync(id, updatedStock);
        }

        public async Task<List<Stock>> UserStocksAsync(string cnp)
        {
            return await stockRepository.UserStocksAsync(cnp);
        }

        public async Task<List<HomepageStock>> GetFilteredAndSortedStocksAsync(string query, string sortOption, bool favoritesOnly, string userCNP)
        {
            var allStocks = await homepageStocksRepo.GetAllAsync(userCNP);
            var filteredStocks = allStocks.Where(stock =>
                stock.StockDetails.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                stock.StockDetails.Symbol.Contains(query, StringComparison.CurrentCultureIgnoreCase));

            if (favoritesOnly)
            {
                filteredStocks = filteredStocks.Where(stock => stock.IsFavorite);
            }

            return sortOption switch
            {
                "Sort by Name" => [.. filteredStocks.OrderBy(stock => stock.StockDetails.Name)],
                "Sort by Price" => [.. filteredStocks.OrderBy(stock => stock.StockDetails.Price)],
                "Sort by Change" => [.. filteredStocks.OrderBy(stock => stock.Change)],
                _ => [.. filteredStocks]
            };
        }

        public async Task AddToFavoritesAsync(HomepageStock stock)
        {
            stock.IsFavorite = true;
            await homepageStocksRepo.UpdateAsync(stock.Id, stock);
        }

        public async Task RemoveFromFavoritesAsync(HomepageStock stock)
        {
            stock.IsFavorite = false;
            await homepageStocksRepo.UpdateAsync(stock.Id, stock);
        }
    }
}
