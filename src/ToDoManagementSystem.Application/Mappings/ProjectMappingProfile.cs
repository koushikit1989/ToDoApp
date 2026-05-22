using AutoMapper;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Mappings;

/// <summary>AutoMapper profile mapping Project entities to/from their DTOs.</summary>
public class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        CreateMap<Project, ProjectResponse>()
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count));
    }
}
