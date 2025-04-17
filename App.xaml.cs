namespace StockApp
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.UI.Xaml;
    using StockApp.Database;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            DatabaseHelper.InitializeDatabase();
            this.InitializeComponent();

            // explanation before the OnUnhandledException method
            this.UnhandledException += this.OnUnhandledException;
        }

        public static Window CurrentWindow { get; set; }

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
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.mainWindow = new MainWindow();
            CurrentWindow = this.mainWindow;
            this.mainWindow.Activate();
        }

        // i found some stupid ass error for the debugger, got it twice and couldn't
        // recreate it ever since thus this method exists if someone finds it there is something to see
        // i assume it's because how the db is or some shit, happens when you exit the app and it crashes the debugger
        // and opens up a new VS window for the project
        // NO IDEA HOW THAT HAPPENS
        // if you find it pls fix it (if it's in the news stuff cuz i fucked something up tell me and i'll fix it)
        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
            System.Diagnostics.Debug.WriteLine(e.Exception.StackTrace);
        }
    }
}
