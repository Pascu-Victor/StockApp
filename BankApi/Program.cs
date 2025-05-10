using BankApi.Data;
using BankApi.Repositories;
using BankApi.Seeders;
using Microsoft.EntityFrameworkCore;
using StockApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure DbContext
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// Register repository
builder.Services.AddScoped<IBaseStocksRepository, BaseStocksRepository>();
builder.Services.AddScoped<IChatReportRepository, ChatReportRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IHomepageStockRepository, HomepageStockRepository>();
builder.Services.AddScoped<IGemStoreRepository, GemStoreRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IInvestmentsRepository, InvestmentsRepository>();
builder.Services.AddScoped<IBillSplitReportRepository, BillSplitReportRepository>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IChatReportRepository, ChatReportRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<ITipsRepository, TipsRepository>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

Type[] seederTypes =
[
    typeof(UsersSeeder),
    typeof(ChatReportsSeeder),
    typeof(LoanRequestsSeeder),
    typeof(LoansSeeder),
    typeof(BillSplitReportsSeeder),
    typeof(InvestmentsSeeder),
    typeof(TransactionLogTransactionsSeeder),
    typeof(ActivityLogsSeeder),
    typeof(AlertsSeeder),
    typeof(BaseStocksSeeder),
    typeof(CreditScoreHistoriesSeeder),
    typeof(GemStoresSeeder),
    typeof(TipsSeeder),
    typeof(TriggeredAlertsSeeder),
    typeof(HomepageStocksSeeder),
    typeof(UserStocksSeeder),
    typeof(GivenTipsSeeder),
];

// Dependency injection for seeders.
foreach (var seederType in seederTypes)
{
    builder.Services.AddSingleton(seederType, sp =>
        Activator.CreateInstance(seederType, sp.GetRequiredService<IConfiguration>())!);
}

var app = builder.Build();

// Apply migrations in development environment
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate(); // This will apply any pending migrations

    // Seed the database
    foreach (var seederType in seederTypes)
    {
        var seeder = (TableSeeder)scope.ServiceProvider.GetRequiredService(seederType);
        await seeder.SeedAsync();
    }

    // Configure the HTTP request pipeline
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
