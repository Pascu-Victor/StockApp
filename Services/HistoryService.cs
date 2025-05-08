namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StockApp.Exceptions;
    using StockApp.Models;

    public class HistoryService : IHistoryService
    {
        private readonly HistoryApiService _apiService;

        public HistoryService(HistoryApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        public List<CreditScoreHistory> GetAllHistory()
        {
            try
            {
                return _apiService.GetAllHistory();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving all credit score history", ex);
            }
        }

        public CreditScoreHistory GetHistoryById(int id)
        {
            try
            {
                return _apiService.GetHistoryById(id);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error retrieving history entry with ID {id}", ex);
            }
        }

        public void AddHistory(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            if (history.Score < 0 || history.Score > 1000)
            {
                throw new HistoryServiceException("Credit score must be between 0 and 1000");
            }

            try
            {
                _apiService.AddHistory(history);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error adding credit score history entry", ex);
            }
        }

        public void UpdateHistory(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            if (history.Score < 0 || history.Score > 1000)
            {
                throw new HistoryServiceException("Credit score must be between 0 and 1000");
            }

            try
            {
                _apiService.UpdateHistory(history);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error updating history entry with ID {history.Id}", ex);
            }
        }

        public void DeleteHistory(int id)
        {
            try
            {
                _apiService.DeleteHistory(id);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error deleting history entry with ID {id}", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return _apiService.GetHistoryForUser(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving credit score history for user", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryWeekly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return _apiService.GetHistoryWeekly(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving weekly credit score history", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryMonthly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return _apiService.GetHistoryMonthly(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving monthly credit score history", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryYearly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return _apiService.GetHistoryYearly(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving yearly credit score history", ex);
            }
        }
    }
}
