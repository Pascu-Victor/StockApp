using BankApi.Data;
using BankApi.Repositories;
using BankApi.Repositories.Impl;
using BankApi.Seeders;
using BankApi.Services;
using Common.Models; // Required for User
using Common.Services;
using Common.Services.Impl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity; // Required for Identity
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure DbContext
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        // Configure identity options here (e.g., password complexity)
        options.SignIn.RequireConfirmedAccount = false; // Example: disable email confirmation for simplicity
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6; // Example: set a minimum password length
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders(); // Adds token providers for password reset, email confirmation, etc.

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    // Only enable JWT validation if configuration is present
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = !string.IsNullOrEmpty(jwtKey),
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = !string.IsNullOrEmpty(jwtKey)
            ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            : null
    };

    // Add this block to read JWT from cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("jwt_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// Configure session services
builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register repository
builder.Services.AddScoped<IBaseStocksRepository, BaseStocksRepository>();
builder.Services.AddScoped<IChatReportRepository, ChatReportRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IHomepageStockRepository, HomepageStockRepository>();
builder.Services.AddScoped<IGemStoreRepository, GemStoreRepository>();
builder.Services.AddScoped<IInvestmentsRepository, InvestmentsRepository>();
builder.Services.AddScoped<IBillSplitReportRepository, BillSplitReportRepository>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<ITipsRepository, TipsRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IStockPageRepository, StockPageRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

// Register services
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IBillSplitReportService, BillSplitReportService>();
builder.Services.AddScoped<IChatReportService, ChatReportService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IInvestmentsService, InvestmentsService>();
builder.Services.AddScoped<ILoanRequestService, LoanRequestService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IStockPageService, StockPageService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<ITipsService, TipsService>();
builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProfanityChecker, ProfanityChecker>();
builder.Services.AddHttpClient<IProfanityChecker, ProfanityChecker>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
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
    typeof(TipsSeeder),
    typeof(TriggeredAlertsSeeder),
    typeof(HomepageStocksSeeder),
    typeof(UserStocksSeeder),
    typeof(GivenTipsSeeder),
];

// Dependency injection for seeders.
foreach (var seederType in seederTypes)
{
    // Updated to pass IServiceProvider to seeder constructors
    builder.Services.AddSingleton(seederType, sp =>
        Activator.CreateInstance(seederType, sp.GetRequiredService<IConfiguration>(), sp)!);
}

var app = builder.Build();

// Apply migrations in development environment
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider; // Get IServiceProvider

    var dbContext = serviceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate(); // This will apply any pending migrations

    // Seed the database
    foreach (var seederType in seederTypes)
    {
        // Resolve seeder using the more specific type to ensure correct constructor is used
        if (serviceProvider.GetService(seederType) is TableSeeder seeder)
        {
            await seeder.SeedAsync();
        }
    }

    // Ensure all users have the default User role
    var userService = serviceProvider.GetRequiredService<IUserService>();
    int updatedUsers = await userService.AddDefaultRoleToAllUsersAsync();
    Console.WriteLine($"Added default User role to {updatedUsers} users");
}

app.UseHttpsRedirection();

app.UseRouting(); // Ensure UseRouting is called before UseAuthentication and UseAuthorization

app.UseAuthentication(); // Add Authentication middleware
app.UseAuthorization(); // Add Authorization middleware

app.UseSession(); // Add Session middleware

app.UseCors("AllowAll");

app.MapControllers();


app.Use(async (context, next) =>
{
    // Get all role claims for the current user
    var user = context.User; // Access the ClaimsPrincipal from the HttpContext
    IEnumerable<Claim> roleClaims = user.Claims.Where(c => c.Type == ClaimTypes.Role);

    // Extract the role values
    IEnumerable<string> roles = roleClaims.Select(c => c.Value);
    // Now you can iterate through the roles
    foreach (var role in roles)
    {
        Console.WriteLine($"User is in role: {role}");
    }

    await next(); // Call the next middleware in the pipeline
});

app.Run();
