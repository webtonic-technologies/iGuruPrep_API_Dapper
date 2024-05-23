using ControlPanel_API.Repository.Implementations;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Implementations;
using ControlPanel_API.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(connectionString));
builder.Services.AddTransient<IDesignationServices, DesignationServices>();
builder.Services.AddTransient<IDesignationRepository, DesignationRepository>();
builder.Services.AddTransient<IFeedbackServices, FeedbackServices>();
builder.Services.AddTransient<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddTransient<IMagazineServices, MagazineServices>();
builder.Services.AddTransient<IMagazineRepository, MagazineRepository>();
builder.Services.AddTransient<IRolesServices, RolesServices>();
builder.Services.AddTransient<IRolesRepository, RolesRepository>();
builder.Services.AddTransient<IStoryOfTheDayServices, StoryOfTheDayServices>();
builder.Services.AddTransient<IStoryOfTheDayRepository, StoryOfTheDayRepository>();
builder.Services.AddTransient<ITicketServices, TicketServices>();
builder.Services.AddTransient<ITicketRepository, TicketRepository>();
builder.Services.AddTransient<INotificationRepository, NotificationRepository>();
builder.Services.AddTransient<INotificationServices, NotificationServices>();
builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddTransient<IEmployeeServices, EmployeeServices>();
builder.Services.AddTransient<IHelpFAQRepository, HelpFAQRepository>();
builder.Services.AddTransient<IHelpFAQServices, HelpFAQServices>();
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
