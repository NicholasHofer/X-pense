using CommandLine;

namespace X_pense;

public class Program
{
    public static void Main(string[] args)
    {
    Parser.Default.ParseArguments<AddOptions, ListOptions, DeleteOptions, SummaryOptions, BudgetOptions>(args).MapResult(
            (AddOptions o) => { AddExpense(o.Description, o.Amount, o.Category); return 0; },
            (ListOptions o) => { ListExpenses(o.Category); return 0; },
            (DeleteOptions o) => { DeleteExpense(o.Id); return 0; },
            (SummaryOptions o) => { GetSummary(o.Category, o.Month); return 0; },
            (BudgetOptions o) => { SetBudget(o.Month, o.Amount);  return 0; },
            errs => 1
        );
    }

    [Verb("add", HelpText = "Add a new expense.")]
    public class AddOptions
    {
        [Option('d', "description", Required = true, HelpText = "Description of the expense.")]
        public required string Description { get; set; }

        [Option('a', "amount", Required = true, HelpText = "Amount of the expense.")]
        public decimal Amount { get; set; }

        [Option('c', "category", Required = true, HelpText = "Category of the expense.")]
        public required string Category {get; set;}
    }

    static void AddExpense(string description, decimal amount, string category)
    {
        Functions.Add(description, amount, category);
    }

    [Verb("list", HelpText = "List all expenses.")]
    public class ListOptions
    {
        [Option('c', "category", Required = false, HelpText = "Category to filter by (optional).")]
        public string? Category { get; set; }
    }

    static void ListExpenses(string? category)
    {
        Functions.List(category);
    }

    [Verb("delete", HelpText = "Delete an expense by ID.")]
    public class DeleteOptions
    {
        [Option('i', "id", Required = true, HelpText = "ID of expense to delete.")]
        public int Id { get; set; }
    }

    static void DeleteExpense(int id)
    {
        Functions.Delete(id);
    }

    [Verb("summary", HelpText = "Get total expenses by month.")]
    public class SummaryOptions
    {
        [Option('m', "month", Required = false, HelpText = "Month number to summarize (optional).")]
        public int Month { get; set; }
        [Option('c', "category", Required = false, HelpText = "Category to summarize (optional).")]
        public string? Category { get; set; }
    }

    static void GetSummary(string? category, int month)
    {
        if (month < 0 || month > 12)
        {
            Console.WriteLine("Month number not valid. Must be between 1-12.");
            return;
        }
        Functions.Summary(category, month);
    }

    [Verb("budget", HelpText = "Set budget amount for specified month.")]
    public class BudgetOptions
    {
        [Option('m', "month", Required = true, HelpText = "Month number to set budget for.")]
        public int Month { get; set; }
        [Option('a', "amount", Required = true, HelpText = "Amount to set budget for.")]
        public decimal Amount { get; set; }
    }

    static void SetBudget(int month, decimal amount)
    {
        Functions.SetBudget(month, amount);
    }
}