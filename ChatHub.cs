using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MessengerSignalR {
    [Authorize]
    public class ChatHub : Hub {
        public async Task Send(string message, string to) {
            // получение текущего пользователя, который отправил сообщение
            //var userName = Context.UserIdentifier;
            var mes = new Message();

            if (to == "") {
                await Clients.All.SendAsync("Receive", message, Context.UserIdentifier + " to all");
                mes = new Message { dateTime = DateTime.UtcNow, receiver = "all", sender = Context.UserIdentifier, textMessage = message };
            }
            else {
                if (Context.UserIdentifier is string userName) {
                    await Clients.Users(to, userName).SendAsync("Receive", message, userName + " to " + to);
                    mes = new Message { dateTime = DateTime.UtcNow, receiver = to, sender = Context.UserIdentifier, textMessage = message };
                }

            }


            using (ApplicationContext db = new ApplicationContext()) {
                db.Messages.Add(mes);
                db.SaveChanges();
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