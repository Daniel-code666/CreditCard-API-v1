using System.ComponentModel.DataAnnotations.Schema;

namespace API_Credit_Card.Models.Dtos
{
    public class CreditCardDto
    {
        public int CardId { get; set; }

        public string CardNumber { get; set; }

        public string ExpiringDate { get; set; }

        public string CVV { get; set; }

        public string UserId { get; set; }

        public UserDataDto User { get; set; }
    }
}
