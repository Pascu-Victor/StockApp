﻿namespace StockApp
{
    using Common.Services;
    using Common.Services.Impl;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.UI.Xaml;
    using StockApp.Pages;
    using StockApp.Services;
    using StockApp.ViewModels;
    using StockApp.Views;
    using StockApp.Views.Components;
    using StockApp.Views.Pages;
    using System;
    using System.Net.Http;

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
            ConfigureHost();

            // explanation before the OnUnhandledException method
            // this.UnhandledException += this.OnUnhandledException;
        }

        private static void ConfigureHost()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Build configuration
                    var configBuilder = new ConfigurationBuilder()
                        .AddUserSecrets<App>()
                        .AddEnvironmentVariables();

                    var config = configBuilder.Build();
                    string apiBaseUrl = App.Configuration.GetValue<string>("ApiBase")
                        ?? throw new InvalidOperationException("API base URL is not configured");

                    services.AddHttpClient("ApiClient", client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    });

                    services.AddSingleton<IConfiguration>(config);

                    // Register the authentication service first
                    services.AddSingleton<IAuthenticationService>(sp =>
                        new AuthenticationService(
                            sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"),
                            sp.GetRequiredService<IConfiguration>()));

                    // Register the AuthenticationDelegatingHandler to automatically handle JWT tokens
                    services.AddTransient<AuthenticationDelegatingHandler>();

                    // Other Services
                    services.AddScoped<ITransactionLogService, TransactionLogProxy>();
                    services.AddScoped<IChatReportService, ChatReportProxyService>();
                    services.AddScoped<IHistoryService, HistoryProxyService>();
                    services.AddScoped<IBillSplitReportService, BillSplitReportProxyService>();
                    services.AddScoped<ILoanService, LoanProxyService>();
                    services.AddScoped<ILoanRequestService, LoanRequestProxyService>();
                    services.AddScoped<IStockPageService, StockPageProxyService>();
                    services.AddScoped<IInvestmentsService, InvestmentsProxyService>();
                    services.AddScoped<IUserService, UserProxyService>();
                    services.AddScoped<IActivityService, ActivityProxyService>();
                    services.AddScoped<IStoreService, StoreProxyService>();
                    services.AddScoped<INewsService, NewsProxyService>();
                    services.AddScoped<IStockService, StockProxyService>();
                    services.AddScoped<IAlertService, AlertProxyService>();
                    services.AddScoped<IMessagesService, MessagesProxyService>();
                    services.AddScoped<ITipsService, TipsProxyService>();
                    services.AddScoped<IProfanityChecker, ProfanityChecker>();

                    // Configure HttpClients with the AuthenticationDelegatingHandler
                    services.AddHttpClient<IStockService, StockProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IInvestmentsService, InvestmentsProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IUserService, UserProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IActivityService, ActivityProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IStoreService, StoreProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<INewsService, NewsProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IStockPageService, StockPageProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IAlertService, AlertProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ILoanService, LoanProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ILoanRequestService, LoanRequestProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IBillSplitReportService, BillSplitReportProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IMessagesService, MessagesProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IHistoryService, HistoryProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ITipsService, TipsProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IChatReportService, ChatReportProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ITransactionLogService, TransactionLogProxy>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    // Don't add the handler to the authentication service's HttpClient
                    // to avoid circular dependencies
                    services.AddSingleton<MainWindow>();

                    // UI Components
                    services.AddTransient<BillSplitReportViewModel>();
                    services.AddTransient<BillSplitReportViewModel>();
                    services.AddTransient<BillSplitReportComponent>();
                    services.AddTransient<BillSplitReportPage>();
                    services.AddTransient<ChatReportComponent>();
                    services.AddTransient<LoanRequestComponent>();
                    services.AddTransient<LoanComponent>();
                    services.AddTransient<BillSplitReportComponent>();
                    services.AddTransient<LoansView>();
                    services.AddTransient<LoanRequestView>();
                    services.AddTransient<UserInfoComponent>();
                    services.AddTransient<UsersView>();
                    services.AddTransient<UserProfileComponent>();
                    services.AddTransient<AlertsView>();
                    services.AddTransient<LoginComponent>();

                    // ViewModels
                    services.AddTransient<StoreViewModel>();
                    services.AddTransient<ProfilePageViewModel>();
                    services.AddTransient<InvestmentsViewModel>();
                    services.AddTransient<HomepageViewModel>();
                    services.AddTransient<CreateStockViewModel>();
                    services.AddTransient<CreateProfilePageViewModel>();
                    services.AddTransient<TipHistoryViewModel>();
                    services.AddTransient<NewsDetailViewModel>();
                    services.AddTransient<NewsListViewModel>();
                    services.AddTransient<NewsListPage>();
                    services.AddTransient<ArticleCreationViewModel>();
                    services.AddTransient<AdminNewsViewModel>();
                    services.AddTransient<StockPageViewModel>();
                    services.AddTransient<TransactionLogViewModel>();
                    services.AddTransient<UserProfileComponentViewModel>();
                    services.AddTransient<AlertViewModel>();
                    services.AddTransient<UpdateProfilePageViewModel>();
                    services.AddTransient<AuthenticationViewModel>();

                    // Pages
                    services.AddTransient<CreateStockPage>();
                    services.AddTransient<TransactionLogPage>();
                    services.AddTransient<ProfilePage>();
                    services.AddTransient<GemStoreWindow>();
                    services.AddTransient<CreateProfilePage>();
                    services.AddTransient<HomepageView>();
                    services.AddTransient<InvestmentsView>();
                    services.AddTransient<TipHistoryWindow>();
                    services.AddTransient<NewsArticleView>();
                    services.AddTransient<LoansViewModel>();
                    services.AddTransient<ChatReportView>();
                    services.AddTransient<ArticleCreationView>();
                    services.AddTransient<AdminNewsControlView>();
                    services.AddTransient<StockPage>();
                    services.AddTransient<UpdateProfilePage>();
                    services.AddTransient<LoginPage>();

                    // FIXME: remove \/\/\/\/
                    services.AddTransient<Func<LoanRequestComponent>>(sp => () => sp.GetRequiredService<LoanRequestComponent>());
                    services.AddTransient<Func<ChatReportComponent>>(sp => () => sp.GetRequiredService<ChatReportComponent>());
                    services.AddTransient<Func<UserInfoComponent>>(sp => () => sp.GetRequiredService<UserInfoComponent>());
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
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="launchActivatedEventArgs">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs)
        {
            MainAppWindow = Host.Services.GetRequiredService<MainWindow>();
            MainAppWindow.Activate();
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {unhandledExceptionEventArgs.Exception.Message}");
            System.Diagnostics.Debug.WriteLine(unhandledExceptionEventArgs.Exception.StackTrace);
        }
    }
}