using API_Credit_Card.Models.Dtos;

namespace API_Credit_Card.Repository.IRepository
{
    public interface ICreditCardRepository
    {
        ICollection<CreditCardDto> GetAllCreditCards();

        CreditCardDto GetCreditCard(int CardId, string UserEmail);

        CreditCardListDto GetCreditCardsFromUser(string UserEmail);

        CreditCardDto CreateCreditCard(CrediCardCreateDto creditCard, string UserEmail);

        CreditCardDto DeleteCreditCard(int CardId, string UserEmail);
    }
}
