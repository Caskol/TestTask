using Microsoft.EntityFrameworkCore;
using TestTask.Services;
using TestTask.Services.Implementations;
using TestTask.SqlData;

var builder = WebApplication.CreateBuilder(args);

//Подключение Entity Framework
string connection = builder.Configuration.GetConnectionString("PostgreSQLConnection");

builder.Services.AddDbContext<AppDbContext>(context => context.UseNpgsql(connection));
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<ICategoryService, CategoryServiceImpl>();
builder.Services.AddTransient<IDailyEventService,DailyEventsServiceImpl>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

var app = builder.Build();

app.MapControllers();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();

app.Run();
