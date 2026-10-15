
using Splitwise.Models;

namespace Splitwise.Strategies;

public interface ISplitStrategy
{
    List<Split> GenerateSplits(
        string expenseId,
        User paidBy,
        long amountPaise,
        IReadOnlyList<User> participants);
}
