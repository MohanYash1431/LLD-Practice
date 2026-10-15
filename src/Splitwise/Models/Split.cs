
namespace Splitwise.Models;

public class Split
{
    public User PaidBy { get; }
    public User PaidFor { get; }
    public long AmountPaise { get; }
    public string ExpenseId { get; }

    public Split(
        User paidBy,
        User paidFor,
        long amountPaise,
        string expenseId)
    {
        PaidBy = paidBy;
        PaidFor = paidFor;
        AmountPaise = amountPaise;
        ExpenseId = expenseId;
    }
}
