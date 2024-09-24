using AutoMapper;
using DiceApi.Data;
using DiceApi.Data.Admin;
using DiceApi.Data.Api;
using DiceApi.Data.Api.Model;
using DiceApi.Data.ApiModels;
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
                .ForMember(dest => dest.Ballance, opt => opt.MapFrom(src => Math.Round(src.Ballance, 2)))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.RefferalPercent, opt => opt.MapFrom(src => src.ReferalPercent))
                .ForMember(dest => dest.IsTelegramUser, opt => opt.MapFrom(src => src.TelegramUserId != null && src.TelegramUserId != 0))
                .ForMember(dest => dest.BlockReason, opt => opt.MapFrom(src => src.BlockReason));

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
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate.AddHours(3).ToString("G")))
                .ForMember(dest => dest.LastActiveDate, opt => opt.MapFrom(src => src.LastAuthDate.AddHours(3).ToString("G")))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => !src.IsActive))
                .ForMember(dest => dest.BlockReason, opt => opt.MapFrom(src => src.BlockReason))
                .ForMember(dest => dest.PaymentForWithdrawal, opt => opt.MapFrom(src => src.PaymentForWithdrawal))
                .ForMember(dest => dest.LastAuthIp, opt => opt.MapFrom(src => src.LastAuthIp.Replace(":fffff:", string.Empty)))
                .ForMember(dest => dest.RegistrationIpAddres, opt => opt.MapFrom(src => src.RegistrationIp.Replace(":fffff:", string.Empty)))
                .ForMember(dest => dest.RefferalPercent, opt => opt.MapFrom(src => src.ReferalPercent))
                .ForMember(dest => dest.EnabledWithrowal, opt => opt.MapFrom(src => src.EnableWithdrawal));

            CreateMap<User, UserReferral>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.ReferalSum))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate));

            CreateMap<UserRegisterResponce, UserRegistrationModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId));

            CreateMap<UserTelegramRegisterResponce, UserRegistrationModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId));

            CreateMap<Cell, CellApiModel>()
                .ForMember(destination => destination.IsOpen, opt => opt.MapFrom(source => source.IsOpen))
                .ForMember(destination => destination.X, opt => opt.MapFrom(source => source.X))
                .ForMember(destination => destination.Y, opt => opt.MapFrom(source => source.Y));


            CreateMap<ActiveMinesGame, MinesGame>()
                .ForMember(destination => destination.UserId, opt => opt.MapFrom(source => source.UserId))
                .ForMember(destination => destination.Sum, opt => opt.MapFrom(source => source.BetSum))
                .ForMember(destination => destination.CanWin, opt => opt.MapFrom(source => source.CanWin))
                .ForMember(destination => destination.Win, opt => opt.MapFrom(source => source.FinishGame));



        }
    }
}
