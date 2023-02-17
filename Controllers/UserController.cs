using API_Credit_Card.Models;
using API_Credit_Card.Models.Dtos;
using API_Credit_Card.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using XAct.Messages;

namespace API_Credit_Card.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private ApiResponse _apiResponse;

        public UserController(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _apiResponse = new();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                var newUser = await _userRepo.Register(userRegisterDto);

                if (newUser.FullName != userRegisterDto.FullName)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(newUser.FullName);
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = newUser;
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

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            try
            {
                var user = await _userRepo.Login(userLoginDto);

                if (user.User.Email != userLoginDto.Email || string.IsNullOrEmpty(user.Token))
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(user.User.Email);
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = user;
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
    }
}
