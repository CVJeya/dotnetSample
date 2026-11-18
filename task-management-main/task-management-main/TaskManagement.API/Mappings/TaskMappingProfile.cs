using AutoMapper;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Mappings;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskEntity, TaskResponseDto>();

        CreateMap<CreateTaskDto, TaskEntity>()
            .ForMember(d => d.CreatedOn, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.LastUpdatedOn, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.LastUpdatedBy, o => o.MapFrom(s => s.CreatedBy));

        CreateMap<UpdateTaskDto, TaskEntity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedOn, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.LastUpdatedOn, o => o.MapFrom(_ => DateTime.UtcNow));
    }
}
