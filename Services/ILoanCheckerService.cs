namespace StockApp.Services
{
    using System;
    using System.Threading.Tasks;

    public interface ILoanCheckerService
    {
        event EventHandler LoansUpdated;

        void Start();

        void Stop();
    }
}
