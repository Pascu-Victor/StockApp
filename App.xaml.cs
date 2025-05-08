// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StockApp
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.UI.Xaml;
    using Src.Data;
    using StockApp.Database;
    using StockApp.Pages;
    using StockApp.Repositories;
    using StockApp.Repositories.Api;
    using StockApp.Services;
    using StockApp.Views;
    using StockApp.Views.Components;
    using StockApp.Views.Pages;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow? MainAppWindow { get; private set; } = null!;

        public static IHost Host { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Note: We've replaced DatabaseHelper.InitializeDatabase() with EF Core migrations
            this.InitializeComponent();
            this.ConfigureHost();

            // explanation before the OnUnhandledException method
            //this.UnhandledException += this.OnUnhandledException;
        }

        private void ConfigureHost()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) =>
                {
                    var config = new ConfigurationBuilder().AddUserSecrets<App>().AddEnvironmentVariables().Build();
                    services.AddSingleton<IConfiguration>(config);
                    services.AddSingleton(new DatabaseConnection());

                    // Configure EF Core
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(ConnectionString));

                    // HttpClient for API communication
                    services.AddHttpClient("BankApi", client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    // Repositories
                    services.AddScoped<IBaseStocksRepository>(provider =>
                    {
                        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                        var client = httpClientFactory.CreateClient("BankApi");
                        return new BankStocksProxyRepo(client);
                    });

                    services.AddScoped<IAlertRepository, AlertProxyRepo>();

                    services.AddSingleton<IActivityRepository, ActivityRepository>();
                    services.AddSingleton<IBillSplitReportRepository, BillSplitReportRepository>();
                    services.AddSingleton<IChatReportRepository, ChatReportRepoProxy>();
                    services.AddSingleton<IHistoryRepository, HistoryRepository>();
                    services.AddSingleton<IInvestmentsRepository, InvestmentsRepository>();
                    services.AddSingleton<ILoanRepository, LoanRepository>();
                    services.AddSingleton<ILoanRequestRepository, LoanRequestRepository>();
                    services.AddSingleton<IUserRepository, UserRepository>();
                    services.AddSingleton<IGemStoreRepository, GemStoreRepository>();

                    // Other Services
                    services.AddSingleton<IActivityService, ActivityService>();
                    services.AddSingleton<IBillSplitReportService, BillSplitReportService>();
                    services.AddSingleton<IChatReportService, ChatReportService>();
                    services.AddSingleton<IHistoryService, HistoryService>();
                    services.AddSingleton<IInvestmentsService, InvestmentsService>();
                    services.AddSingleton<ILoanCheckerService, LoanCheckerService>();
                    services.AddSingleton<ILoanRequestService, LoanRequestService>();
                    services.AddSingleton<ILoanService, LoanService>();
                    services.AddSingleton<IMessagesService, MessagesService>();
                    services.AddSingleton<ITipsService, TipsService>();
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<IZodiacService, ZodiacService>();

                    // Gem Store Services
                    services.AddSingleton<IGemStoreApiService>(provider =>
                    {
                        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                        var client = httpClientFactory.CreateClient("BankApi");
                        return new GemStoreApiService(client);
                    });
                    services.AddSingleton<IGemStoreService, GemStoreService>();
                    services.AddSingleton<IStoreService, StoreService>();

                    services.AddSingleton<MainWindow>();

                    // UI Components
                    services.AddTransient<BillSplitReportComponent>();
                    services.AddTransient<Func<BillSplitReportComponent>>(provider =>
                    {
                        return () => provider.GetRequiredService<BillSplitReportComponent>();
                    });
                    services.AddTransient<BillSplitReportPage>();

                    services.AddTransient<ChatReportComponent>();
                    services.AddTransient<Func<ChatReportComponent>>(provider =>
                    {
                        return () => provider.GetRequiredService<ChatReportComponent>();
                    });
                    services.AddTransient<ChatReportView>();

                    services.AddTransient<LoanComponent>();
                    services.AddTransient<Func<LoanComponent>>(provider =>
                    {
                        return () => provider.GetRequiredService<LoanComponent>();
                    });
                    services.AddTransient<LoansView>();

                    services.AddTransient<LoanRequestComponent>();
                    services.AddTransient<Func<LoanRequestComponent>>(provider =>
                    {
                        return () => provider.GetRequiredService<LoanRequestComponent>();
                    });
                    services.AddTransient<LoanRequestView>();

                    services.AddTransient<UserInfoComponent>();
                    services.AddTransient<Func<UserInfoComponent>>(provider =>
                    {
                        return () => provider.GetRequiredService<UserInfoComponent>();
                    });
                    services.AddTransient<UsersView>();

                    services.AddTransient<NewsListPage>();
                    services.AddTransient<CreateStockPage>();
                    services.AddTransient<TransactionLogPage>();
                    services.AddTransient<ProfilePage>();
                    services.AddTransient<GemStoreWindow>();
                    services.AddTransient<CreateProfilePage>();
                    services.AddTransient<HomepageView>();
                }).Build();
        }


        /// <summary>
        /// Gets or sets the current window of the application.
        /// </summary>
        public static Window CurrentWindow { get; set; } = null!;

        /// <summary>
        /// Gets Configuration object for the application.
        /// </summary>
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        /// <summary>
        /// Gets ConnectionString string for the database.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the connection string is not set in appsettings.json.</exception>
        public static string ConnectionString { get; } =
            Configuration.GetConnectionString("StockApp_DB") ??
            throw new InvalidOperationException("Connection string is not set in appsettings.json");

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="launchActivatedEventArgs">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs)
        {
            MainAppWindow = Host.Services.GetRequiredService<MainWindow>();
            MainAppWindow.Activate();
        }

        // i found some stupid ass error for the debugger, got it twice and couldn't
        // recreate it ever since thus this method exists if someone finds it there is something to see
        // i assume it's because how the db is or some shit, happens when you exit the app and it crashes the debugger
        // and opens up a new VS window for the project
        // NO IDEA HOW THAT HAPPENS
        // if you find it pls fix it (if it's in the news stuff cuz i fucked something up tell me and i'll fix it)
        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {unhandledExceptionEventArgs.Exception.Message}");
            System.Diagnostics.Debug.WriteLine(unhandledExceptionEventArgs.Exception.StackTrace);
        }
    }
}
