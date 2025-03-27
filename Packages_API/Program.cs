using Packages_API.Repository.Implementations;
using Packages_API.Repository.Interfaces;
using Packages_API.Services.Implementations;
using Packages_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));
// Add services to the container.
builder.Services.AddTransient<ICoinsServices, CoinsServices>();
builder.Services.AddTransient<ICoinsRepository, CoinsRepository>();

builder.Services.AddTransient<IModuleWiseServices, ModuleWiseServices>();
builder.Services.AddTransient<IModuleWiseRepository, ModuleWiseRepository>();

builder.Services.AddTransient<ISubscriptionPackageServices, SubscriptionPackageServices>();
builder.Services.AddTransient<ISubscriptionPackageRepository, SubscriptionPackageRepository>();


builder.Services.AddTransient<ICoinsConfigurationServices, CoinsConfigurationServices>();
builder.Services.AddTransient<ICoinsConfigurationRepository, CoinsConfigurationRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
