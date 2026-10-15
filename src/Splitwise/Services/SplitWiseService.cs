
using Splitwise.Models;
using Splitwise.Strategies;

namespace Splitwise.Services;

public class SplitWiseService
{
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Group> _groups = new();
    private readonly object _lock = new();

    public User CreateUser(string id, string name)
    {
        lock (_lock)
        {
            User user = new User(id, name);
            _users.Add(id, user);
            return user;
        }
    }

    public Group CreateGroup(
        string id,
        string name,
        List<User> members)
    {
        lock (_lock)
        {
            Group group = new Group(id, name);

            foreach (User user in members)
                group.AddMember(user);

            _groups.Add(id, group);
            return group;
        }
    }

    public Expense AddExpense(
        string groupId,
        string expenseId,
        User paidBy,
        long amountPaise,
        List<User> participants,
        ISplitStrategy strategy)
    {
        lock (_lock)
        {
            Group group = _groups[groupId];

            if (amountPaise <= 0 || participants.Count == 0)
                throw new ArgumentException("Invalid expense.");

            if (group.Expenses.Any(e => e.Id == expenseId))
                throw new InvalidOperationException("Duplicate expense.");

            if (participants.Select(u => u.Id).Distinct().Count()
                != participants.Count)
                throw new ArgumentException("Duplicate participants.");

            if (!participants.Any(u => u.Id == paidBy.Id))
                throw new ArgumentException("Payer must participate.");

            if (!group.Members.Any(u => u.Id == paidBy.Id) ||
                participants.Any(u =>
                    !group.Members.Any(m => m.Id == u.Id)))
                throw new ArgumentException("Invalid group member.");

            // 1. Create Expense
            Expense expense = new Expense(
                expenseId, paidBy, amountPaise,
                participants, strategy);

            // 2. Generate Splits
            var splits = expense.CalculateShare();

            // 3. Update Balances
            group.BalanceManager.UpdateBalanceSheet(splits);

            // 4. Record Expense
            group.AddExpense(expense);

            return expense;
        }
    }
    
public void ShowBalance(string groupId, User user)
{
    lock (_lock)
    {
        Group group = _groups[groupId];

        var balances = group.BalanceManager.GetBalance(user.Id!);

        Console.WriteLine($"\nBalances for {user.Name}:");

        bool hasBalance = false;

        foreach (var entry in balances)
        {
            string otherUserId = entry.Key;
            long amount = entry.Value;

            if (amount == 0)
                continue;

            hasBalance = true;

            string? otherName = _users[otherUserId].Name;

            if (amount > 0)
            {
                Console.WriteLine(
                    $"{otherName} owes {user.Name} ₹{amount / 100m:F2}");
            }
            else
            {
                Console.WriteLine(
                    $"{user.Name} owes {otherName} ₹{-amount / 100m:F2}");
            }
        }

        if (!hasBalance)
        {
            Console.WriteLine("No pending balances.");
        }
    }
}

}
