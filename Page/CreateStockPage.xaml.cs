using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StockApp.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CreateStock
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateStockPage : Microsoft.UI.Xaml.Controls.Page
    {
        public CreateStockPage()
        {
            this.InitializeComponent();
            this.DataContext = new CreateStockViewModel();
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

    }
}
