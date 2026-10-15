
using Splitwise.Models;

namespace Splitwise.Strategies;

public class EqualSplit : ISplitStrategy
{
    public List<Split> GenerateSplits(
        string expenseId,
        User paidBy,
        long amountPaise,
        IReadOnlyList<User> participants)
    {
        if (participants.Count == 0)
            throw new ArgumentException("No participants.");

        if (amountPaise <= 0)
            throw new ArgumentException("Amount must be positive.");

        List<Split> splits = new();

        long share = amountPaise / participants.Count;
        long remainder = amountPaise % participants.Count;

        for (int i = 0; i < participants.Count; i++)
        {
            User user = participants[i];

            long userShare = share + (i < remainder ? 1 : 0);

            if (user.Id == paidBy.Id)
                continue;

            splits.Add(new Split(
                paidBy, user, userShare, expenseId));
        }

        return splits;
    }
}
