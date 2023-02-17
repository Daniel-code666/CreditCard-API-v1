using API_Credit_Card.Models;
using API_Credit_Card.Models.Dtos;
using AutoMapper;

namespace API_Credit_Card.AppMapper
{
    public class AppMapper : Profile
    {
        public AppMapper()
        {
            //CreateMap<User, UserRegisterDto>().ReverseMap();
            CreateMap<User, UserDataDto>().ReverseMap();
            CreateMap<CreditCard, CreditCardDto>().ReverseMap();
            CreateMap<CreditCard, CrediCardCreateDto>().ReverseMap();
            CreateMap<CrediCardCreateDto, CreditCardDto>().ReverseMap();
        }
    }
}
