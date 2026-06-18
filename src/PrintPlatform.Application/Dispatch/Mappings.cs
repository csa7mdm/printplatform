using Mapster;
using PrintPlatform.Domain.Dispatch;

namespace PrintPlatform.Application.Dispatch;

public class Mappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<JobAssignment, JobAssignmentDto>();
        config.NewConfig<QCRecord, QCRecordDto>();
    }
}
