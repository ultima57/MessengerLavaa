namespace MessengerSignalR {
    public class Message {
        public int Id { get; set; }
        public string sender { get; set; }
        public string receiver { get; set; }
        public string textMessage { get; set; }
        public DateTime dateTime { get; set; }


    }
}
