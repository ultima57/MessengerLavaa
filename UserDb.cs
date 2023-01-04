namespace MessengerSignalR {
    public class UserDb {
        public int Id { get; set; }
        public string Login { get; set; }
        public string? Password { get; set; }



    }

    record class Person(string Email, string Password);
}