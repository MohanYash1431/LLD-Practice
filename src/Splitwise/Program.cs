
using Splitwise.Models;
using Splitwise.Services;
using Splitwise.Strategies;

// Step 1: Create SplitWiseService
SplitWiseService service = new();

// Step 2: Create Users
User alice = service.CreateUser("u1", "Alice");
User bob = service.CreateUser("u2", "Bob");
User carol = service.CreateUser("u3", "Carol");

// Step 3: Create Group
Group goaTrip = service.CreateGroup(
    "g1",
    "Goa Trip",
    new List<User> { alice, bob, carol }
);

// Step 4: Add Expense using EqualSplit
// Alice pays Rs.900 for Alice, Bob and Carol
service.AddExpense(
    "g1",
    "e1",
    alice,
    90000,
    new List<User> { alice, bob, carol },
    new EqualSplit()
);

// Step 5: Add Expense using PercentageSplit
// Bob pays Rs.200: Alice 30%, Carol 70%
service.AddExpense(
    "g1",
    "e2",
    bob,
    20000,
    new List<User> { alice, bob, carol },
    new PercentageSplit(
        new Dictionary<string, decimal>
        {
            { "u1", 30m },
            { "u3", 70m }
        }
    )
);

// Step 6: Display Group Details
Console.WriteLine($"Group: {goaTrip.Name}");
Console.WriteLine($"Total Expenses: {goaTrip.Expenses.Count}");

// Step 7: Display Balances
Console.WriteLine("\n--- Group Balances ---");

service.ShowBalance("g1", alice);
service.ShowBalance("g1", bob);
service.ShowBalance("g1", carol);


Console.WriteLine("\n--- Duplicate Expense Test ---");

try
{
    service.AddExpense(
        "g1",
        "e1", // Already exists!
        alice,
        90000,
        new List<User> { alice, bob, carol },
        new EqualSplit()
    );

    Console.WriteLine("ERROR: Duplicate was accepted!");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Duplicate blocked: {ex.Message}");
}

Console.WriteLine(
    $"Total Expenses: {goaTrip.Expenses.Count}");

    
Console.WriteLine("\n--- Balance Consistency Test ---");

var aliceBalance = goaTrip.BalanceManager.GetBalance(alice.Id!);
var bobBalance = goaTrip.BalanceManager.GetBalance(bob.Id!);

long aliceToBob = aliceBalance.GetValueOrDefault(bob.Id);
long bobToAlice = bobBalance.GetValueOrDefault(alice.Id);

Console.WriteLine($"Alice -> Bob: {aliceToBob}");
Console.WriteLine($"Bob -> Alice: {bobToAlice}");

bool isConsistent = aliceToBob == -bobToAlice;

Console.WriteLine($"Balance Consistent: {isConsistent}");

