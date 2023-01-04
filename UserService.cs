using MessengerSignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class UserService {
    ApplicationContext db;
    IDistributedCache cache;

    public UserService() {
    }

    public UserService(ApplicationContext context, IDistributedCache distributedCache) {
        db = context;
        cache = distributedCache;
    }
    public async Task<UserDb?> GetUser(int id) {
        Console.WriteLine(id);
        UserDb? user = null;
        // пытаемся получить данные из кэша по Login
        //Console.WriteLine(await cache.GetStringAsync(Login.ToString()));
        var userString = await cache.GetStringAsync(id.ToString());
        //десериализируем из строки в объект Useredb
        if (userString != null) user = JsonSerializer.Deserialize<UserDb>(userString);

        // если данные не найдены в кэше
        if (user == null) {
            // обращаемся к базе данных
            user = await db.Users.FindAsync(id);
            // если пользователь найден, то добавляем в кэш
            if (user != null) {
                Console.WriteLine($"{user.Id} извлечен из base");
                // сериализуем данные в строку в формате json
                userString = JsonSerializer.Serialize(user);
                // сохраняем строковое представление объекта в формате json в кэш на 2 минуты
                await cache.SetStringAsync(user.Id.ToString(), userString, new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });
            }
        }
        else {
            Console.WriteLine($"{user.Login} извлечен из cache");
        }
        return user;
    }
}