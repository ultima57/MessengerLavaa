using Microsoft.AspNetCore.Authorization; // для атрибута Authorize
using Microsoft.AspNetCore.SignalR;

namespace MessengerSignalR {
    [Authorize]
    public class ChatHub : Hub {
        public async Task Send(string message, string to) {
            // получение текущего пользователя, который отправил сообщение
            //var userName = Context.UserIdentifier;

            if (to == "") {
                await Clients.All.SendAsync("Receive", message, Context.UserIdentifier + " to all");
            }
            else {
                if (Context.UserIdentifier is string userName) {
                    await Clients.Users(to, userName).SendAsync("Receive", message, userName + " to " + to);
                }

            }
        }

        public override async Task OnConnectedAsync() {
            await Clients.All.SendAsync("Notify", $"Приветствуем {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception) {
            await Clients.All.SendAsync("Notify", $"{Context.UserIdentifier} покинул в чат");
            await base.OnDisconnectedAsync(exception);
        }

    }
}