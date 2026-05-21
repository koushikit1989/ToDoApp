using AutoMapper;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;

namespace ToDoManagementSystem.Application.Mappings;

/// <summary>AutoMapper profile mapping TaskItem entities to/from their DTOs.</summary>
public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskResponse>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src =>
                src.DueDate < DateTime.UtcNow && src.Status != DomainTaskStatus.Completed));

        CreateMap<CreateTaskRequest, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => (TaskPriority)src.Priority))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => DomainTaskStatus.Pending));
    }
}
