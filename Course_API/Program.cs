using Course_API.Repository.Implementations;
using Course_API.Repository.Interfaces;
using Course_API.Services.Implementations;
using Course_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));
builder.Services.AddTransient<IBookServices, BookServices>();
builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IContentMasterRepository, ContentMasterRepository>();
builder.Services.AddTransient<IContentMasterServices, ContentMasterServices>();
builder.Services.AddTransient<ISyllabusRepository, SyllabusRepository>();
builder.Services.AddTransient<ISyllabusServices, SyllabusServices>();
builder.Services.AddTransient<ITestSeriesRepository, TestSeriesRepository>();
builder.Services.AddTransient<ITestSeriesServices, TestSeriesServices>();
builder.Services.AddTransient<IScholarshipTestServices, ScholarshipTestServices>();
builder.Services.AddTransient<IScholarshipTestRepository, ScholarshipTestRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("corsapp");

app.UseAuthorization();

app.MapControllers();

app.Run();
