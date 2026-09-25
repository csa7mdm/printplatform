using MediatR;
using PrintPlatform.Application.Webhooks.Models;
using PrintPlatform.Domain.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace PrintPlatform.Application.Webhooks.Commands;

/// <summary>
/// Command to process an incoming WhatsApp webhook notification.
/// </summary>
public sealed record ProcessWhatsAppMessageCommand(WhatsAppNotification Notification) : IRequest<Result>;

internal sealed class ProcessWhatsAppMessageCommandHandler : IRequestHandler<ProcessWhatsAppMessageCommand, Result>
{
    public Task<Result> Handle(ProcessWhatsAppMessageCommand request, CancellationToken cancellationToken)
    {
        // Place holder for processing logic (e.g., parsing messages, saving to DB, triggering events).
        return Task.FromResult(Result.Success());
    }
}
