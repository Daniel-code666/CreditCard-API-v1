using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Credit_Card.Models.Dtos
{
    public class CrediCardCreateDto
    {
        public int CardId { get; set; }

        [Required(ErrorMessage = "El número es necesario")]
        public string CardNumber { get; set; }

        public string? ExpiringDate { get; set; }

        [Required(ErrorMessage = "El número es necesario")]
        public string CVV { get; set; }

        public string? UserId { get; set; }
    }
}
