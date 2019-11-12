using System;
using System.Linq;
using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                opt => { opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url); }
            )
            .ForMember(
                dest => dest.Age,
                opt => { 
                    //............................CEMBO - code it in here:
                    //opt.ResolveUsing(d => {
                    //    var age = DateTime.Today.Year - d.DateOfBirth.Year;
                    //    if(DateTime.Today < dateTime.AddYears(age))
                    //    { age--; };
                    //    return age;
                    //});
                    ////... or using the extension method to prevent code repeating
                    opt.ResolveUsing( d => d.DateOfBirth.Age() );
                }
            );


            CreateMap<User, UserForDetailedDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                opt => { opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url); }
            )
            .ForMember(
                dest => dest.Age,
                opt => { 
                    //opt.ResolveUsing(d => {
                    //    var age = DateTime.Today.Year - d.DateOfBirth.Year;
                    //    if(DateTime.Today < dateTime.AddYears(age))
                    //    { age--; };
                    //    return age;
                    //});
                    ////... or using the extension method to prevent code repeating
                    opt.ResolveUsing( d => d.DateOfBirth.Age() );
                }
            );

            
            CreateMap<Photo, PhotoForDetailedDto>();

            CreateMap<UserForUpdateDto, User>();

            CreateMap<Photo, PhotoForReturnDto>();

            CreateMap<PhotoForCreationDto, Photo>();

            CreateMap<UserForRegisterDto, User>();

            // ... .ReverseMap() instead of creating second reverse map.
            CreateMap<MessageForCreationDto, Message>().ReverseMap();

            CreateMap<Message, MessageToReturnDto>()
                .ForMember(dto => dto.SenderPhotoUrl, x => x.MapFrom(m => m.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dto => dto.RecipientPhotoUrl, x => x.MapFrom(m => m.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));

        }
    }
}