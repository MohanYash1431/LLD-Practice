using Splitwise.Models;

namespace Splitwise.Services;

public class BalanceManagerService
{
    private readonly Dictionary<string, Dictionary<string, long>>
        _balances = new();

    private readonly object _lock = new();

    private void EnsureUser(string userId)
    {
        if (!_balances.ContainsKey(userId))
        {
            _balances[userId] = new Dictionary<string, long>();
        }
    }

    public void UpdateBalanceSheet(IReadOnlyList<Split> splits)
    {
        lock (_lock)
        {
            foreach (Split split in splits)
            {
                string creditor = split.PaidBy.Id ??
                    throw new ArgumentException("Split creditor ID cannot be null.");
                string debtor = split.PaidFor.Id ??
                    throw new ArgumentException("Split debtor ID cannot be null.");
                long amount = split.AmountPaise;

                if (creditor == debtor)
                    continue;

                if (amount < 0)
                    throw new ArgumentException("Invalid split amount.");

                EnsureUser(creditor);
                EnsureUser(debtor);

                _balances[creditor][debtor] =
                    _balances[creditor].GetValueOrDefault(debtor) + amount;

                _balances[debtor][creditor] =
                    _balances[debtor].GetValueOrDefault(creditor) - amount;
            }
        }
    }

    public IReadOnlyDictionary<string, long> GetBalance(string userId)
    {
        lock (_lock)
        {
            if (!_balances.ContainsKey(userId))
                return new Dictionary<string, long>();

            return new Dictionary<string, long>(_balances[userId]);
        }
    }
}
