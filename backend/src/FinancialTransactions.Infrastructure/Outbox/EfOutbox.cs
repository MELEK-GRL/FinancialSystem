using System.Text.Json;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Infrastructure.Persistence;

namespace FinancialTransactions.Infrastructure.Outbox;

public sealed class EfOutbox(AppDbContext dbContext) : IOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue<TPayload>(string eventType, Guid aggregateId, TPayload payload) =>
        dbContext.OutboxMessages.Add(OutboxMessage.Create(eventType, aggregateId, JsonSerializer.Serialize(payload, SerializerOptions)));
}
