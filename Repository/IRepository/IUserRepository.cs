using API_Credit_Card.Models.Dtos;

namespace API_Credit_Card.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<UserDataDto> Register(UserRegisterDto userRegisterDto);

        Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    }
}
