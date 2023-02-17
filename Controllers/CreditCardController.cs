using API_Credit_Card.Models;
using API_Credit_Card.Models.Dtos;
using API_Credit_Card.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace API_Credit_Card.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "credit_card")]
    [ApiController]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardRepository _cardRepo;
        private readonly IMapper _mapper;
        private ApiResponse _apiResponse;
        private string _secretKey;
        private readonly IConfiguration _config;

        public CreditCardController(ICreditCardRepository cardRepo, IMapper mapper, IConfiguration config)
        {
            _mapper = mapper;
            _cardRepo = cardRepo;
            _apiResponse = new();
            _config = config;
            _secretKey = _config.GetValue<string>("ApiSettings:Secret");
        }

        [Authorize(Roles = "user")]
        [HttpGet]
        public IActionResult GetAllCards()
        {
            try
            {
                var cardList = _cardRepo.GetAllCreditCards();

                var firstCardListElement = cardList.First();

                if (firstCardListElement.ExpiringDate == "false")
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(firstCardListElement.CardNumber);
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = cardList;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
                return BadRequest(_apiResponse);
            }
        }

        [Authorize(Roles = "user")]
        [HttpGet]
        public IActionResult GetCrediCard(int CardId)
        {
            try
            {
                var card = _cardRepo.GetCreditCard(CardId, GetEmailFromToken());

                if (!string.IsNullOrEmpty(card.CardNumber))
                {
                    if (card.ExpiringDate == "false")
                    {
                        _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                        _apiResponse.IsSuccess = false;
                        _apiResponse.ErrorMessages.Add(card.CardNumber);
                        return BadRequest(_apiResponse);
                    }

                    _apiResponse.StatusCode = HttpStatusCode.OK;
                    _apiResponse.IsSuccess = true;
                    _apiResponse.Result = card;
                    return Ok(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                _apiResponse.IsSuccess = true;
                _apiResponse.ErrorMessages.Add("El usuario no tiene ninguna tarjeta con ese id");
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
                return BadRequest(_apiResponse);
            }
        }

        [Authorize(Roles = "user")]
        [HttpGet]
        public IActionResult GetCreditCardFromUser()
        {
            try
            {
                var cardList = _cardRepo.GetCreditCardsFromUser(GetEmailFromToken());

                if (cardList.User != null)
                {
                    var firstCardListElement = cardList.crediCards.FirstOrDefault();

                    if (firstCardListElement.ExpiringDate == "false")
                    {
                        _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                        _apiResponse.IsSuccess = false;
                        _apiResponse.ErrorMessages.Add(firstCardListElement.CardNumber);
                        return BadRequest(_apiResponse);
                    }

                    _apiResponse.StatusCode = HttpStatusCode.OK;
                    _apiResponse.IsSuccess = true;
                    _apiResponse.Result = cardList;
                    return Ok(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                _apiResponse.IsSuccess = true;
                _apiResponse.ErrorMessages.Add("El usuario no tiene tarjetas registradas");
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
                return BadRequest(_apiResponse);
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost]
        public async Task<IActionResult> CreateCreditCard([FromBody] CrediCardCreateDto creditCard)
        {
            try
            {
                if (!ModelState.IsValid || creditCard == null || string.IsNullOrEmpty(GetEmailFromToken()))
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Faltan datos");
                    return BadRequest(_apiResponse);
                }

                var card = _cardRepo.CreateCreditCard(creditCard, GetEmailFromToken());

                if (card.CardNumber != creditCard.CardNumber)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(card.CardNumber);
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = card;
                return Ok(_apiResponse);

            }
            catch (Exception ex)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
                return BadRequest(_apiResponse);
            }
        }

        [Authorize(Roles = "user")]
        [HttpDelete("{CardId}")]
        public IActionResult DeleteCreditCard([FromRoute] int CardId)
        {
            try
            {
                var deletedCard = _cardRepo.DeleteCreditCard(CardId, GetEmailFromToken());

                if (string.IsNullOrEmpty(deletedCard.CardNumber))
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("El usuario no tiene tarjetas con ese id");
                    return Ok(_apiResponse);
                }

                if (deletedCard.ExpiringDate == "false")
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(deletedCard.CardNumber);
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = deletedCard;
                return Ok(_apiResponse);
            }
            catch(Exception ex)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
                return BadRequest(_apiResponse);
            }
        }

        private string GetEmailFromToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("ApiSettings:Secret"));
            var token = HttpContext.Request.Headers["Authorization"];

            token = token.ToString().Substring(7);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var UserEmail = jwtToken.Claims.First(x => x.Type == "unique_name").Value;

            return UserEmail;
        }
    }
}
