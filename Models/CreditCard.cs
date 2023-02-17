using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Credit_Card.Models
{
    public class CreditCard
    {
        [Key]
        public int CardId { get; set; }

        public string CardNumber { get; set; }

        public string ExpiringDate { get; set; }

        public string CVV { get; set; }

        [ForeignKey("OwnerId")]
        public string UserId { get; set; }

        public User User { get; set; }
    }
}
