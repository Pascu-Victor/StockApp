namespace BankApi.Repositories.Articles
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApi.Data;
    using BankApi.Models;
    using BankApi.Models.Articles;
    using Microsoft.EntityFrameworkCore;

    public class NewsArticleRepository : INewsArticleRepository
    {
        private readonly ApiDbContext _context;

        public NewsArticleRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<List<NewsArticle>> GetAllNewsArticlesAsync() =>
            await _context.NewsArticles.ToListAsync();

        public async Task<List<NewsArticle>> GetNewsArticlesByCategoryAsync(string category) =>
            await _context.NewsArticles.Where(article => article.Category == category).ToListAsync();

        public async Task<List<NewsArticle>> GetNewsArticlesByStockAsync(string stockName)
        {
            return await _context.BaseStocks
                .Where(stock => stock.Name == stockName)
                .Join(
                    _context.NewsArticleStocks,
                    stock => stock.Id,
                    newsArticleStock => newsArticleStock.StockId,
                    (stock, newsArticleStock) => newsArticleStock.Article)
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetNewsArticleByIdAsync(int articleId) =>
            await _context.NewsArticles.FindAsync(articleId);

        public async Task AddNewsArticleAsync(NewsArticle article)
        {
            await _context.NewsArticles.AddAsync(article);
            await _context.SaveChangesAsync();
        }

        public async Task AddRelatedStocksAsync(int articleId, List<int> stockIds)
        {
            await _context.NewsArticleStocks.AddRangeAsync(stockIds.Select(stockId =>
            {
                return new NewsArticleStock
                {
                    ArticleId = articleId,
                    StockId = stockId
                };
            }));
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNewsArticleAsync(NewsArticle newsArticle)
        {
            _context.NewsArticles.Update(newsArticle);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteNewsArticleAsync(int articleId)
        {
            var article = await _context.NewsArticles.FindAsync(articleId);
            if (article == null)
            {
                return false;
            }

            _context.NewsArticles.Remove(article);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkNewsArticleAsReadAsync(int articleId)
        {
            var article = await _context.NewsArticles.FindAsync(articleId);
            if (article == null)
            {
                return false;
            }

            article.IsRead = true;
            _context.NewsArticles.Update(article);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
