namespace PLinkage.Models
{
    public class Chat
    {
        public Guid ChatId { get; set; } = Guid.NewGuid();
        public List<Guid> MessengerId { get; set; } = new List<Guid>();
        public List<Message> Messages { get; set; } = new List<Message>();
    }

    public class Message
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public int MessageOrder { get; set; } = 0;
        public Guid SenderId { get; set; } = Guid.Empty;
        public Guid ReceiverId { get; set; } = Guid.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public DateTime MessageDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
