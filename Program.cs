using MessengerSignalR;   // пространство имен класса ChatHub
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;







using (ApplicationContext db = new ApplicationContext()) {
    Console.WriteLine("ss");
    //var user1 = new UserDb { Login = "Tom", Password = "1" };
    //var user2 = new UserDb { Login = "Alice", Password = "1" };
    //var user3 = new UserDb { Login = "Rob", Password = "1" };

    //// добавляем их в бд
    //db.Users.AddRange(user1, user2, user3);

    //Console.WriteLine("ss2");
    //db.SaveChanges();
}
// получение данных
using (ApplicationContext db = new ApplicationContext()) {
    // получаем объекты из бд и выводим на консоль
    var users = db.Users.ToList();
    Console.WriteLine("Users list:");
    foreach (var u in users) {
        Console.WriteLine($"{u.Id}.{u.Login} - {u.Password}");
    }
}


// условная бд с пользователями
var people = new List<Person>
 {
    new Person("tom@gmail.com", "1"),
    new Person("bob@gmail.com", "55555"),
    new Person("sam@gmail.com", "22222")
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>(); // Устанавливаем сервис для получения Id пользователя

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                var accessToken = context.Request.Query["access_token"];

                // если запрос направлен хабу
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat")) {
                    // получаем токен из строки запроса
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSignalR();

var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();   // добавление middleware аутентификации 
app.UseAuthorization();   // добавление middleware авторизации 


app.MapPost("/login", (Person loginModel) => {
    // находим пользователя 
    Person? person = people.FirstOrDefault(p => p.Email == loginModel.Email && p.Password == loginModel.Password);
    // если пользователь не найден, отправляем статусный код 401
    if (person is null) return Results.Unauthorized();

    var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, person.Email) };
    // создаем JWT-токен
    var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    // формируем ответ
    var response = new {
        access_token = encodedJwt,
        username = person.Email
    };

    return Results.Json(response);
});

app.MapHub<ChatHub>("/chat");
app.Run();

record class Person(string Email, string Password);
public class AuthOptions {
    public const string ISSUER = "MyAuthServer"; // издатель токена
    public const string AUDIENCE = "MyAuthClient"; // потребитель токена
    const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}