using System;
using AutoMapper;
using WebApplication.Models;
using WebApplication.Models.DomainModels;
using WebApplication.Models.ViewModels;

namespace WebApplication
{
    public class AutoMapperConfig
    {
        // регистрируем мапперы
        public static void RegisterMappings()
        {
            //Mapper.Initialize(map =>
            //{
            //    map.CreateMap<ApplicationUser, PeopleViewModel>();
            //});
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<PeopleViewModel, ApplicationUser>();
            //});
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<RegisterViewModel, ApplicationUser>()
            //        .ForMember("Gender", opt => opt.MapFrom(u => (u.Gender == true) ? "Мужской" : "Женский"))
            //        .ForMember("UserName", opt => opt.MapFrom(u => u.Email))
            //        .ForMember("Email", opt => opt.MapFrom(u => u.Email))
            //        .ForMember("Name", opt => opt.MapFrom(u => u.Name))
            //        .ForMember("Surname", opt => opt.MapFrom(u => u.Surname))
            //        .ForMember("Country", opt => opt.MapFrom(u => u.Country))
            //        .ForMember("City", opt => opt.MapFrom(u => u.City))
            //        .ForMember("Avatar", opt => opt.MapFrom(u => u.Avatar))
            //        .ForMember("Age", opt => opt.MapFrom(u => u.Age));
            //});
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<ApplicationUser, UserPageViewModel>()
            //        .ForMember("Gender", opt => opt.MapFrom(u => (u.Gender == true) ? "Мужской" : "Женский"));
            //});
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<UserPageViewModel, ApplicationUser>()
            //        .ForMember("Gender", opt => opt.MapFrom(u => (u.Gender == "Мужской")));
            //});
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<Event, EventViewModel>();

            //});

            Mapper.AssertConfigurationIsValid();
            Mapper.CreateMap<ApplicationUser, PeopleViewModel>();
            Mapper.CreateMap<PeopleViewModel, ApplicationUser>();
            Mapper.CreateMap<ApplicationUser, UserPageViewModel>()
                .ForMember("Gender", opt => opt.MapFrom(u => (u.Gender == true) ? "Мужской" : "Женский"));
            Mapper.CreateMap<UserPageViewModel, ApplicationUser>()
                .ForMember("Gender", opt => opt.MapFrom(u => u.Gender == "Мужской"));
            Mapper.CreateMap<Event, EventViewModel>();
        }
    }
}