using API_Credit_Card.Data;
using API_Credit_Card.Models;
using API_Credit_Card.Models.Dtos;
using API_Credit_Card.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using User = API_Credit_Card.Models.User;

namespace API_Credit_Card.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private string _secretKey;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, IConfiguration config, RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager, IMapper mapper)
        {
            _db = db;
            _secretKey = config.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
        {
            try
            {
                var user = _db.User.FirstOrDefault(u => u.Email.ToLower() == userLoginDto.Email.ToLower());

                bool isUserValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

                if (user == null || !isUserValid)
                {
                    return new UserLoginResponseDto()
                    {
                        Token = "",
                        User = new()
                        {
                            Email = "Usuario no encontrado"
                        }
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);

                var tknHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Email.ToString()),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tknHandler.CreateToken(tokenDescriptor);

                UserLoginResponseDto userLoginResponseDto = new UserLoginResponseDto()
                {
                    Token = tknHandler.WriteToken(token),
                    User = _mapper.Map<UserDataDto>(user)
                };

                return userLoginResponseDto;
            }
            catch(Exception ex)
            {
                UserLoginResponseDto userLoginResponseDto = new UserLoginResponseDto()
                {
                    Token = "",
                    User =
                    {
                        Email = ex.Message
                    }
                };

                return userLoginResponseDto;
            }
        }

        public async Task<UserDataDto> Register(UserRegisterDto userRegisterDto)
        {
            try
            {
                if (CheckEmail(userRegisterDto.Email))
                {
                    return new UserDataDto()
                    {
                        FullName = "El correo " + userRegisterDto.Email + " ya está en uso"
                    };
                }

                User user = new()
                {
                    FullName = userRegisterDto.FullName,
                    Email = userRegisterDto.Email,
                    NormalizedEmail = userRegisterDto.Email.ToUpper(),
                    UserName = userRegisterDto.Email
                };

                await _userManager.CreateAsync(user, userRegisterDto.Password);

                if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole("admin"));
                    await _roleManager.CreateAsync(new IdentityRole("user"));
                }

                await _userManager.AddToRoleAsync(user, "user");
                var userRetorned = _db.User.FirstOrDefault(u => u.UserName == userRegisterDto.Email);

                return _mapper.Map<UserDataDto>(userRetorned);
            } 
            catch (Exception ex)
            {
                return new UserDataDto()
                {
                    FullName = ex.Message
                };
            }
        }

        private bool CheckEmail(string email)
        {
            return _db.User.Any(u => u.Email == email);
        }
    }
}
