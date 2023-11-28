using AutoMapper;
using DiceApi.Data;
using DiceApi.Data.Admin;
using DiceApi.Data.Api;
using DiceApi.Data.Api.Model;
using DiceApi.Data.Data.Dice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserApi>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Ballance, opt => opt.MapFrom(src => src.Ballance))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
            
            CreateMap<User, AdminUserInfo>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Ballance, opt => opt.MapFrom(src => src.Ballance));

            CreateMap<User, AdminUser>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Ballance, opt => opt.MapFrom(src => src.Ballance))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => !src.IsActive))
                .ForMember(dest => dest.RefferalPercent, opt => opt.MapFrom(src => src.ReferalPercent));

            CreateMap<User, UserReferral>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.ReferalSum))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate));

            
        }
    }
}
