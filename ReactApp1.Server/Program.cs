using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReactApp1.Server;
using ReactApp1.Server.Data;
using ReactApp1.Server.Health;
using ReactApp1.Server.Models.Authentication;
using ReactApp1.Server.Services;
using Serilog;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

  
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });


    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImagesService, ImagesService>();
builder.Services.AddSingleton<IHealthCheck,DatabaseHealthCheck>();
builder.Services.AddSingleton<IHealthCheck, ApiHealthCheck>();
builder.Services.AddHttpClient<CurrencyService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

builder.Services.AddDbContext<ShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});




builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});

builder.Services.AddHealthChecks().
    AddCheck<DatabaseHealthCheck>(nameof(DatabaseHealthCheck))
    .AddCheck<ApiHealthCheck>(nameof(ApiHealthCheck));



builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(5); //Sets the time interval in which HealthCheck will be triggered
    options.MaximumHistoryEntriesPerEndpoint(10); //Sets the maximum number of records displayed in history
    options.AddHealthCheckEndpoint("Health Checks API", "/healthcheck"); //Sets the Health Check endpoint
}).
AddInMemoryStorage();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).    
    CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles(); 


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthCheck v1"));

    app.UseCors(x => x
     .AllowAnyMethod()
     .AllowAnyHeader()
     .SetIsOriginAllowed(origin => true) 
     .AllowCredentials()); 
}

app.UseSerilogRequestLogging();

app.UseAuthorization();
app.UseRouting();
app.MapHub<CurrencyHub>("/hub");



app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapHealthChecks("/health", new HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecks("/database_health", new HealthCheckOptions()
    {
        Predicate = (check) => check.Name == nameof(DatabaseHealthCheck),
        ResponseWriter = HealthCheckExtensions.WriteResponse
    });
    endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecks("/api_health", new HealthCheckOptions()
    {
        Predicate = (check) => check.Name == nameof(ApiHealthCheck),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecksUI(options => options.UIPath = "/dashboard");

  

});




app.MapFallbackToFile("/index.html");


app.Run();


