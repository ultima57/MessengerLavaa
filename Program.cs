using MessengerSignalR;
using Microsoft.AspNetCore.SignalR;

var people = new List<Person>
 {
    new Person("tom@gmail.com", "1"),
    new Person("bob@gmail.com", "55555"),
    new Person("sam@gmail.com", "22222")
};

using (ApplicationContext db = new ApplicationContext()) {

    var users = db.Users.ToList();

    foreach (var u in users) {
        Console.WriteLine();
        people.Add(new Person(u.Login, u.Password.ToString()));
    }
}

var builder = WebApplication.CreateBuilder();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>(); // Устанавливаем сервис для получения Id пользователя
builder.Services.AddDbContext<ApplicationContext>();
builder.Services.AddAuthorization();

Controllers.BuilderServicesAddAuthentication(builder);

builder.Services.AddSignalR();
// внедрение зависимости UserService
builder.Services.AddTransient<UserService>();
// добавление кэширования
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = "localhost";
    options.InstanceName = "local";
});

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

Controllers.AppMapGet(app);

Controllers.AppMapPostRegistation(people, app);

Controllers.AppMapPostLogin(people, app);

app.MapHub<ChatHub>("/chat");
app.Run();

