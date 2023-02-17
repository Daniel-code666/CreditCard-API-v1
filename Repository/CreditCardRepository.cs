using API_Credit_Card.Data;
using API_Credit_Card.Models;
using API_Credit_Card.Models.Dtos;
using API_Credit_Card.Repository.IRepository;
using AutoMapper;

namespace API_Credit_Card.Repository
{
    public class CreditCardRepository : ICreditCardRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public CreditCardRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public CreditCardDto CreateCreditCard(CrediCardCreateDto creditCard, string UserEmail)
        {
            try
            {
                var user = _db.User.FirstOrDefault(u => u.Email == UserEmail);

                creditCard.ExpiringDate = DateTime.Now.AddYears(4).ToString();
                creditCard.UserId = user.Id;

                var newCard = _mapper.Map<CreditCard>(creditCard);

                var createdCard = _mapper.Map<CreditCardDto>(newCard);

                _db.CreditCard.Add(newCard);

                return Save() ? createdCard : new CreditCardDto() { CardNumber = "Hubo un error"};
            } 
            catch(Exception ex)
            {
                return new CreditCardDto() { CardNumber = ex.Message };
            }
        }

        public ICollection<CreditCardDto> GetAllCreditCards()
        {
            try
            {
                List<CreditCardDto> cardList = new();

                var cardListResult = _db.CreditCard.Join(_db.User, c => c.UserId, u => u.Id, (creditcard, user) => new
                {
                    creditcard,
                    user
                });

                foreach (var item in cardListResult)
                {
                    var card = new CreditCardDto
                    {
                        CardId = item.creditcard.CardId,
                        CardNumber = item.creditcard.CardNumber,
                        ExpiringDate = item.creditcard.ExpiringDate,
                        CVV = item.creditcard.CVV,
                        UserId = item.creditcard.UserId,
                        User = new UserDataDto
                        {
                            Id = item.user.Id,
                            Email = item.user.Email,
                            FullName = item.user.FullName,
                            UserName = item.user.UserName
                        }
                    };

                    cardList.Add(card);
                }

                return cardList;
            } 
            catch(Exception ex)
            {
                var card = new CreditCardDto()
                {
                    ExpiringDate = "false",
                    CardNumber = ex.Message
                };

                List<CreditCardDto> cardList = new();

                cardList.Add(card);

                return cardList;
            }
        }

        public CreditCardDto GetCreditCard(int CardId, string UserEmail)
        {
            try
            {
                var cardResult = _db.CreditCard.Where(c => c.CardId == CardId)
                    .Join(_db.User, c => c.UserId, u => u.Id, (creditcard, user) => new
                {
                    creditcard,
                    user
                }).Where(u => u.user.Email == UserEmail).FirstOrDefault();

                if (cardResult != null)
                {
                    var card = new CreditCardDto()
                    {
                        CardId = cardResult.creditcard.CardId,
                        CardNumber = cardResult.creditcard.CardNumber,
                        ExpiringDate = cardResult.creditcard.ExpiringDate,
                        CVV = cardResult.creditcard.CVV,
                        UserId = cardResult.creditcard.UserId,
                        User = new UserDataDto()
                        {
                            Id = cardResult.user.Id,
                            Email = cardResult.user.Email,
                            FullName = cardResult.user.FullName,
                            UserName = cardResult.user.UserName
                        }
                    };

                    return card;
                }

                return new CreditCardDto();
            }
            catch (Exception ex)
            {
                var card = new CreditCardDto()
                {
                    ExpiringDate = "false",
                    CardNumber = ex.Message
                };

                return card;
            }
        }

        public CreditCardListDto GetCreditCardsFromUser(string UserEmail)
        {
            try
            {
                var CrediCardList = new CreditCardListDto();
                // List<CreditCardListDto> cardList = new();

                var cardListResult = _db.CreditCard.Join(_db.User, c => c.UserId, u => u.Id, (creditcard, user) => new
                    {
                        creditcard,
                        user
                    }).Where(u => u.user.Email == UserEmail);

                foreach (var item in cardListResult)
                {
                    var card = new CrediCardCreateDto
                    {
                        CardId = item.creditcard.CardId,
                        CardNumber = item.creditcard.CardNumber,
                        ExpiringDate = item.creditcard.ExpiringDate,
                        CVV = item.creditcard.CVV,
                        UserId = item.creditcard.UserId
                    };

                    CrediCardList.crediCards.Add(card);
                    CrediCardList.User = _mapper.Map<UserDataDto>(item.user);
                }

                return CrediCardList;
            }
            catch(Exception ex)
            {
                var CrediCardList = new CreditCardListDto();

                var card = new CrediCardCreateDto()
                {
                    ExpiringDate = "false",
                    CardNumber = ex.Message
                };

                CrediCardList.crediCards.Add(card);

                return CrediCardList;
            }
        }

        public CreditCardDto DeleteCreditCard(int CardId, string UserEmail)
        {
            try
            {
                var cardResult = _db.CreditCard.Where(c => c.CardId == CardId)
                    .Join(_db.User, c => c.UserId, u => u.Id, (creditcard, user) => new
                    {
                        creditcard,
                        user
                    }).Where(u => u.user.Email == UserEmail).FirstOrDefault();

                if (cardResult != null)
                {
                    var card = cardResult.creditcard;

                    _db.CreditCard.Remove(card);

                    Save();

                    return _mapper.Map<CreditCardDto>(card);
                }

                return new CreditCardDto();
            }
            catch(Exception ex)
            {
                var card = new CreditCardDto()
                {
                    ExpiringDate = "false",
                    CardNumber = ex.Message
                };

                return card;
            }
        }

        private bool Save()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }
    }
}
