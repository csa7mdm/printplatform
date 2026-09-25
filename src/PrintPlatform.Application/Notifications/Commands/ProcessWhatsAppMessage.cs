using MediatR;
using Microsoft.Extensions.Logging;
using PrintPlatform.Application.Notifications.Models;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Notifications.Commands;

public record ProcessWhatsAppMessageCommand(WhatsAppNotification Notification) : IRequest<Result>;

public class ProcessWhatsAppMessageCommandHandler : IRequestHandler<ProcessWhatsAppMessageCommand, Result>
{
    private readonly ILogger<ProcessWhatsAppMessageCommandHandler> _logger;

    public ProcessWhatsAppMessageCommandHandler(ILogger<ProcessWhatsAppMessageCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result> Handle(ProcessWhatsAppMessageCommand request, CancellationToken cancellationToken)
    {
        if (request.Notification?.Entry == null)
        {
            _logger.LogWarning("Received empty or invalid WhatsApp notification payload.");
            return Task.FromResult(Result.Success());
        }

        foreach (var entry in request.Notification.Entry)
        {
            if (entry.Changes == null) continue;

            foreach (var change in entry.Changes)
            {
                var value = change.Value;
                if (value == null) continue;

                // Log any messages received
                if (value.Messages != null)
                {
                    foreach (var message in value.Messages)
                    {
                        _logger.LogInformation(
                            "Received WhatsApp message {MessageId} of type '{MessageType}' from {From}",
                            message.Id, message.Type, message.From);
                    }
                }

                // Log any status updates received
                if (value.Statuses != null)
                {
                    foreach (var status in value.Statuses)
                    {
                        _logger.LogInformation(
                            "WhatsApp message {MessageId} to recipient {RecipientId} status changed to '{Status}'",
                            status.Id, status.RecipientId, status.Status);
                    }
                }
            }
        }

        return Task.FromResult(Result.Success());
    }
}
