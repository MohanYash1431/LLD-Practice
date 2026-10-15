
using Splitwise.Strategies;

namespace Splitwise.Models;

public class Expense
{
    public string Id { get; }
    public User PaidBy { get; }
    public long AmountPaise { get; }
    public IReadOnlyList<User> Participants { get; }
    public ISplitStrategy Strategy { get; }

    public IReadOnlyList<Split> Splits { get; private set; }
        = Array.Empty<Split>();

    public Expense(
        string id,
        User paidBy,
        long amountPaise,
        IReadOnlyList<User> participants,
        ISplitStrategy strategy)
    {
        Id = id;
        PaidBy = paidBy;
        AmountPaise = amountPaise;
        Participants = participants.ToList().AsReadOnly();
        Strategy = strategy;
    }

    public IReadOnlyList<Split> CalculateShare()
    {
        Splits = Strategy.GenerateSplits(
            Id,
            PaidBy,
            AmountPaise,
            Participants
        ).AsReadOnly();

        return Splits;
    }
}
