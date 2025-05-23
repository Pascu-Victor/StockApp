using Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StockAppWeb.Services;
using System.Text;
// Add for logging
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure authentication
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

    // Log the loaded JWT configuration values for verification
    var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
    var startupLogger = loggerFactory.CreateLogger<Program>();

    if (string.IsNullOrEmpty(jwtKey))
    {
        startupLogger.LogError("JWT Key (Jwt:Key) is not configured in appsettings.json or is empty.");
    }
    if (string.IsNullOrEmpty(jwtIssuer))
    {
        startupLogger.LogWarning("JWT Issuer (Jwt:Issuer) is not configured in appsettings.json or is empty. Depending on validation settings, this might be an issue.");
    }
    if (string.IsNullOrEmpty(jwtAudience))
    {
        startupLogger.LogWarning("JWT Audience (Jwt:Audience) is not configured in appsettings.json or is empty. Depending on validation settings, this might be an issue.");
    }

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
            : null,
        ClockSkew = TimeSpan.FromMinutes(1) // Allow a small clock skew, default is 5 minutes. Adjust if necessary.
    };

    // Configure JWT Bearer to check for token in cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            if (context.Request.Cookies.TryGetValue("jwt_token", out var tokenFromCookie))
            {
                logger.LogInformation("JWT token found in cookie 'jwt_token'. Token: {Token}", tokenFromCookie);
                context.Token = tokenFromCookie;
            }
            else
            {
                logger.LogWarning("JWT token not found in cookie 'jwt_token'.");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            logger.LogError(context.Exception, "JWT Authentication FAILED. Exception: {ExceptionMessage}. Token (if present): {Token}", context.Exception.Message, context.Request.Cookies["jwt_token"]);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            logger.LogInformation("JWT token successfully VALIDATED. User: {User}, Claims: {Claims}", context.Principal?.Identity?.Name, context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}"));
            return Task.CompletedTask;
        }
    };
});

// Register HttpContextAccessor for session access - still needed for WebAuthenticationService to access User
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for BankApi
string apiBaseUrl = builder.Configuration["ApiBase"]
    ?? throw new InvalidOperationException("API base URL is not configured in appsettings.json");

builder.Services.AddHttpClient("BankApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Register services
builder.Services.AddScoped<IAuthenticationService, WebAuthenticationService>();
builder.Services.AddScoped<IUserService, UserProxyService>();
builder.Services.AddScoped<ILoanService, LoanProxyService>();
builder.Services.AddScoped<ILoanRequestService, LoanRequestProxyService>();
builder.Services.AddScoped<IStockService, StockProxyService>();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IUserService, UserProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ILoanService, LoanProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ILoanRequestService, LoanRequestProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IStockService, StockProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();