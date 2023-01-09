using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MessengerSignalR {
    public class Controllers {
        internal static void AppMapGet(WebApplication app) {
            app.MapGet("/user/{id}", async (int id, UserService userService) => {
                UserDb? user = await userService.GetUser(id);
                if (user != null) return $"User {user.Login}  Id={user.Id}  Password={user.Password}";
                return "User not found";
            });
        }
        internal static void AppMapPostRegistation(List<Person> people, WebApplication app) {
            app.MapPost("/reg", async (Person loginModel) => {
                Console.WriteLine(loginModel.ToString());
                if ((loginModel.Email is null) || (loginModel.Email.Equals(""))) {

                    Console.WriteLine("Empty field");

                    return Results.NoContent();
                }
                Person? person = people.FirstOrDefault(p => p.Email == loginModel.Email);
                using (ApplicationContext db = new ApplicationContext()) {

                    var userSearched = db.Users.ToList().FirstOrDefault(p => p.Login == loginModel.Email);

                }

                if (person is not null) {

                    Console.WriteLine("This login already in base");

                    return Results.Conflict();
                }
                using (ApplicationContext db = new ApplicationContext()) {

                    var newUser = new UserDb { Login = loginModel.Email, Password = loginModel.Password };
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    people.Add(new Person(newUser.Login, newUser.Password));

                }

                Console.WriteLine("Created new account");

                return Results.Accepted();

            });
        }
        internal static void AppMapPostLogin(List<Person> people, WebApplication app) {
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
        }
        internal static void BuilderServicesAddAuthentication(WebApplicationBuilder builder) {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        // указывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = AuthOptions.ISSUER,
                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = AuthOptions.AUDIENCE,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,
                        // установка ключа безопасности
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };

                    options.Events = new JwtBearerEvents {
                        OnMessageReceived = context => {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat")) {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
