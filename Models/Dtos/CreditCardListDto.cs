namespace API_Credit_Card.Models.Dtos
{
    public class CreditCardListDto
    {
        public CreditCardListDto()
        {
            crediCards = new List<CrediCardCreateDto>();
        }

        public List<CrediCardCreateDto> crediCards { get; set; }

        public UserDataDto User { get; set; }
    }
}
