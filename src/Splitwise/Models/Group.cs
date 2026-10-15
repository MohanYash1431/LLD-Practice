
using Splitwise.Services;

namespace Splitwise.Models;

public class Group
{
    public string Id { get; }
    public string Name { get; }

    private readonly List<User> _members = new();
    private readonly List<Expense> _expenses = new();

    public IReadOnlyList<User> Members => _members.AsReadOnly();
    public IReadOnlyList<Expense> Expenses => _expenses.AsReadOnly();

    public BalanceManagerService BalanceManager { get; } = new();

    public Group(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddMember(User user)
    {
        if (_members.Any(u => u.Id == user.Id))
            return;

        _members.Add(user);
    }

    public void AddExpense(Expense expense)
    {
        if (_expenses.Any(e => e.Id == expense.Id))
            throw new InvalidOperationException("Expense already exists.");

        _expenses.Add(expense);
    }
}
