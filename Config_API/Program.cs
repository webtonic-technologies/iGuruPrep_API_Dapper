using Config_API.Repository.Implementations;
using Config_API.Repository.Interfaces;
using Config_API.Services.Implementations;
using Config_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));
builder.Services.AddTransient<IBoardServices, BoardServices>();
builder.Services.AddTransient<IBoardRepository, BoardRepository>();
builder.Services.AddTransient<IClassServices, ClassServices>();
builder.Services.AddTransient<IClassRepository, ClassRepository>();
builder.Services.AddTransient<IClassCourseMappingServices, ClassCourseMappingServices>();
builder.Services.AddTransient<IClassCourseMappingRepository, ClassCourseMappingRepository>();
builder.Services.AddTransient<ICourseServices, CourseServices>();
builder.Services.AddTransient<ICourseRepository, CourseRepository>();
builder.Services.AddTransient<IDifficultyLevelServices, DifficultyLevelServices>();
builder.Services.AddTransient<IDifficultyLevelRepository, DifficultyLevelRepository>();
builder.Services.AddTransient<IStatusMessageServices, StatusMessageServices>();
builder.Services.AddTransient<IStatusMessageRepository, StatusMessageRepository>();
builder.Services.AddTransient<ISubjectServices, SubjectServices>();
builder.Services.AddTransient<ISubjectRepository, SubjectRepository>();
builder.Services.AddTransient<INotificationModRepository, NotificationModRepository>();
builder.Services.AddTransient<INotificationModServices, NotificationModServices>();
builder.Services.AddTransient<IQuestionTypeRepository, QuestionTypeRepository>();
builder.Services.AddTransient<IQuestionTypeService, QuestionTypeService>();
builder.Services.AddTransient<ITypeOfTestSeriesRepository, TypeOfTestSeriesRepository>();
builder.Services.AddTransient<ITypeOfTestSeriesServices, TypeOfTestSeriesServices>();
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
