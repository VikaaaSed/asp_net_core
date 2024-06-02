using System.ComponentModel.DataAnnotations;

namespace C_Sharp_lab_4.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public int Id_Sender { get; set; }
        public int Id_Recipient { get; set; }
        public string? Hedder { get; set; }

        public string? TextMessage { get; set; }
        public DateTime DateDispatch { get; set; }

        public bool Status { get; set; }
    }
}
