using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using minimalApi.Data;
using minimalApi.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sqlConnection = new SqlConnectionStringBuilder();
sqlConnection.ConnectionString = builder.Configuration.GetConnectionString("Default");
sqlConnection.UserID = builder.Configuration["UserId"];
sqlConnection.Password = builder.Configuration["Password"];

builder.Services.AddDbContext<CommandDbContext>(option => option.UseSqlServer(sqlConnection.ConnectionString));
builder.Services.AddScoped<IRepository, CommandRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();

