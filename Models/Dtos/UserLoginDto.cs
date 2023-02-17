using System.ComponentModel.DataAnnotations;

namespace API_Credit_Card.Models.Dtos
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "El correo es necesario")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es necesaria")]
        public string Password { get; set; }
    }
}
