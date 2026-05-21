using AutoMapper;
using ToDoManagementSystem.Application.DTOs.Auth;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Mappings;

/// <summary>AutoMapper profile mapping User entities to/from their DTOs.</summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLowerInvariant()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => "User"))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));
    }
}
