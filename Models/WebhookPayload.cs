namespace MetaWhatsAppBot.Models
{
    public class WebhookPayload
    {
        public List<Entry> Entry { get; set; } = new();
    }

    public class Entry
    {
        public List<Change> Changes { get; set; } = new();
    }

    public class Change
    {
        public Value Value { get; set; } = new();
    }

    public class Value
    {
        public List<Message> Messages { get; set; } = new();
    }

    public class Message
    {
        public string From { get; set; } = string.Empty;

        public Text Text { get; set; } = new();
    }

    public class Text
    {
        public string Body { get; set; } = string.Empty;
    }
}
