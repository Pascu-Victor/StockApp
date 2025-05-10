namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Services;

    /// <summary>
    /// ViewModel for the homepage, managing stock display, filtering, sorting, and navigation.
    /// </summary>
    public class HomepageViewModel : INotifyPropertyChanged
    {
        private readonly IHomepageService homepageService;
        private readonly IUserService userService;

        private ObservableCollection<HomepageStock> filteredStocks = [];
        private ObservableCollection<HomepageStock> filteredFavoriteStocks = [];
        private string searchQuery = string.Empty;
        private string selectedSortOption = string.Empty;
        private bool isGuestUser;

        public HomepageViewModel(IHomepageService homepageService, IUserService userService)
        {
            this.homepageService = homepageService ?? throw new ArgumentNullException(nameof(homepageService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));

            this.IsGuestUser = this.userService.IsGuest();
            this.LoadStocksAsync();

            this.FavoriteCommand = new RelayCommand(async obj => await this.ToggleFavoriteAsync(obj as HomepageStock));
            this.SearchCommand = new RelayCommand(async _ => await this.ApplyFilterAndSortAsync());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand FavoriteCommand { get; }

        public bool CanModifyFavorites => this.userService.IsGuest() == false;

        public ICommand SearchCommand { get; }

        public ObservableCollection<HomepageStock> FilteredStocks
        {
            get => this.filteredStocks;
            private set
            {
                this.filteredStocks = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<HomepageStock> FilteredFavoriteStocks
        {
            get => this.filteredFavoriteStocks;
            private set
            {
                this.filteredFavoriteStocks = value;
                this.OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                this.searchQuery = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedSortOption
        {
            get => this.selectedSortOption;
            set
            {
                this.selectedSortOption = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsGuestUser
        {
            get => this.isGuestUser;
            private set
            {
                this.isGuestUser = value;
                this.OnPropertyChanged();
            }
        }

        private async Task LoadStocksAsync()
        {
            var stocks = await this.homepageService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, false);
            this.filteredStocks.Clear();
            foreach (var stock in stocks)
            {
                this.filteredStocks.Add(stock);
            }

            var favoriteStocks = await this.homepageService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, true);
            this.filteredFavoriteStocks.Clear();
            foreach (var stock in favoriteStocks)
            {
                this.filteredFavoriteStocks.Add(stock);
            }
        }

        private async Task ApplyFilterAndSortAsync()
        {
            var stocks = await this.homepageService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, false);
            this.FilteredStocks.Clear();
            foreach (var stock in stocks)
            {
                this.FilteredStocks.Add(stock);
            }

            var favoriteStocks = await this.homepageService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, true);
            this.FilteredFavoriteStocks.Clear();
            foreach (var stock in favoriteStocks)
            {
                this.FilteredFavoriteStocks.Add(stock);
            }
        }

        private async Task ToggleFavoriteAsync(HomepageStock? stock)
        {
            if (stock == null)
            {
                return;
            }

            if (stock.IsFavorite)
            {
                await this.homepageService.RemoveFromFavoritesAsync(stock);
            }
            else
            {
                await this.homepageService.AddToFavoritesAsync(stock);
            }

            await this.ApplyFilterAndSortAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
