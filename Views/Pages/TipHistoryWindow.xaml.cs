namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml;
    using StockApp.Models;
    using StockApp.ViewModels;

    public sealed partial class TipHistoryWindow : Window
    {
        private readonly TipHistoryViewModel viewModel;

        public TipHistoryWindow(TipHistoryViewModel tipHistoryViewModel)
        {
            this.InitializeComponent();
            this.viewModel = tipHistoryViewModel;
            this.TipHistoryContainer.DataContext = this.viewModel;
        }

        public async void LoadUser(User user)
        {
            await this.viewModel.LoadUserData(user);
        }
    }

}
