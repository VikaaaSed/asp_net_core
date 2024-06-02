namespace C_Sharp_lab_4.Models
{
    public class MessageModel: SendMessageModel
    {
        public int Id { get; set; }
        public string? Sender { get; set; }
        public DateTime Date { get; set; }
        public bool Status { get; set; }
    }
}
