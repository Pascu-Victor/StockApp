﻿// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StockApp
{
    using System;
    using System.Collections.Generic;
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
    using StockApp.ViewModels;
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
            // this.UnhandledException += this.OnUnhandledException;
        }

        private void ConfigureHost()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Build configuration
                    var configBuilder = new ConfigurationBuilder()
                        .AddUserSecrets<App>()
                        .AddEnvironmentVariables()
                        .AddInMemoryCollection(new Dictionary<string, string> {
                            { "ApiBaseUrl", "https://localhost:7001/" },
                        });

                    var config = configBuilder.Build();
                    services.AddSingleton<IConfiguration>(config);
                    services.AddSingleton(new DatabaseConnection());

                    // Configure EF Core
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(ConnectionString));

                    // Repositories
                    services.AddScoped<IAlertRepository, AlertProxyRepo>();
                    services.AddScoped<ITransactionRepository, TransactionProxyRepository>();
                    services.AddScoped<ITransactionLogService, TransactionLogService>();
                    services.AddScoped<TransactionLogViewModel>();
                    services.AddScoped<IInvestmentsRepository, InvestmentsProxyRepository>();

                    services.AddSingleton<IBillSplitReportRepository, BillSplitReportProxyRepository>();
                    services.AddSingleton<ITransactionRepository, TransactionProxyRepository>();
                    services.AddSingleton<IChatReportRepository, ChatReportRepoProxy>();
                    services.AddSingleton<ILoanRepository, LoanProxyRepository>();
                    services.AddSingleton<ILoanRequestRepository, LoanRequestProxyRepo>();
                    services.AddSingleton<IActivityRepo, ActivityProxyRepo>();
                    services.AddSingleton<IGemStoreRepository, GemStoreProxyRepo>();
                    services.AddSingleton<IUserRepository, UserProxyRepository>();
                    services.AddSingleton<IProfileRepository, ProfileProxyRepo>();
                    services.AddSingleton<IBaseStocksRepository, BaseStocksProxyRepository>();
                    services.AddSingleton<IHistoryRepository, HistoryProxyRepository>();
                    services.AddSingleton<IHomepageStocksRepository, HomepageStocksProxyRepository>();

                    // HttpClient for API communication
                    services.AddHttpClient<IChatReportRepository, ChatReportRepoProxy>(client =>
                    {
                        client.BaseAddress = new Uri(config["ApiBaseUrl"]);
                    });

                    // Register the API services
                    services.AddScoped<IBaseStocksService, BaseStocksService>();

                    // Register BillSplitReport API service
                    services.AddHttpClient<IBillSplitReportRepository, BillSplitReportProxyRepository>((client) =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });
                    services.AddHttpClient<IAlertRepository, AlertProxyRepo>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    services.AddHttpClient<IActivityRepo, ActivityProxyRepo>(client =>
                    {
                        client.BaseAddress = new Uri(config["ApiBaseUrl"]);
                    });
                    services.AddHttpClient<IUserRepository, UserProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    services.AddHttpClient<IBaseStocksRepository, BaseStocksProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    // Legacy repositories
                    services.AddSingleton<IChatReportRepository, ChatReportRepoProxy>();
                    services.AddSingleton<IStockPageRepository, StockPageRepository>();

                    services.AddHttpClient<IInvestmentsRepository, InvestmentsProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    services.AddHttpClient<IHomepageStocksRepository, HomepageStocksProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });
                    services.AddHttpClient<ILoanRequestRepository, LoanRequestProxyRepo>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });
                    services.AddHttpClient<ILoanRepository, LoanProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    services.AddHttpClient<IProfileRepository, ProfileProxyRepo>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    services.AddHttpClient<IGemStoreRepository, GemStoreProxyRepo>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });

                    services.AddHttpClient<IHistoryRepository, HistoryProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });


                    services.AddHttpClient<ITransactionRepository, TransactionProxyRepository>(client =>
                    {
                        client.BaseAddress = new Uri("https://localhost:7001/");
                    });
                    // Other Services
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
                    services.AddSingleton<IActivityService, ActivityService>();
                    services.AddSingleton<IStoreService, StoreService>();
                    services.AddSingleton<IProfileService, ProfileService>();
                    services.AddSingleton<IHomepageService, HomepageService>();
                    services.AddSingleton<ICreateStockService, CreateStockService>();
                    services.AddSingleton<IStockPageService, StockPageService>();
                    services.AddSingleton<MainWindow>();

                    // UI Components
                    services.AddTransient<BillSplitReportViewModel>();
                    services.AddTransient<BillSplitReportViewModel>();
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

                    // ViewModels

                    services.AddTransient<StoreViewModel>();

                    // Register services for UserInfoComponent
                    services.AddTransient<IHistoryService, HistoryService>();

                    services.AddTransient<NewsListPage>();
                    services.AddTransient<CreateStockPage>();
                    services.AddTransient<TransactionLogPage>();
                    services.AddTransient<ProfilePage>();
                    services.AddTransient<GemStoreWindow>();
                    services.AddTransient<CreateProfilePage>();
                    services.AddTransient<ProfilePageViewModel>();
                    services.AddTransient<HomepageView>();
                    services.AddTransient<InvestmentsViewModel>();
                    services.AddTransient<InvestmentsView>();
                    services.AddTransient<HomepageViewModel>();
                    services.AddTransient<CreateStockViewModel>();
                    services.AddTransient<CreateProfilePageViewModel>();
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
