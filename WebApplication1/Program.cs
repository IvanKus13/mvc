using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;


var builder = WebApplication.CreateBuilder(args);

// Вибір СУБД через appsettings.json
var databaseProvider = builder.Configuration["DatabaseProvider"];

var connectionString = databaseProvider switch
{
    "PostgreSQL" => builder.Configuration.GetConnectionString("PostgreSql"),
    "SqlServer" => builder.Configuration.GetConnectionString("SqlServer"),
    _ => throw new InvalidOperationException("Невідома СУБД.")
};

if (databaseProvider == "PostgreSQL")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
builder.Services.AddDbContext<PostgreSqlDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgreSql")));

builder.Services.AddControllersWithViews();

// для сессій
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // сколько хранить вход
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Стартовий контролер на Cars
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cars}/{action=Index}/{id?}");

// Створення адміна при запуску
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        db.Users.Add(new User
        {
            Login = "admin",
            Password = "1234",
            Role = "Admin"
        });

        db.SaveChanges();
    }
}


app.Run();
