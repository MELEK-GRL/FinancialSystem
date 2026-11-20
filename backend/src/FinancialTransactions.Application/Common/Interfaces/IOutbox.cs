namespace FinancialTransactions.Application.Common.Interfaces;

public interface IOutbox
{
    void Enqueue<TPayload>(string eventType, Guid aggregateId, TPayload payload);
}
