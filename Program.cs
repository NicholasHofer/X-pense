using System.ComponentModel.DataAnnotations;
using CommandLine;

namespace X_pense;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<AddOptions, ListOptions, DeleteOptions, SummaryOptions>(args).MapResult(
            (AddOptions o) => { AddExpense(o.Description, o.Amount);  return 0; },
            (ListOptions o) => { ListExpenses();  return 0; },
            (DeleteOptions o) => { DeleteExpense(o.Id);  return 0; },
            (SummaryOptions o) => { GetSummary(o.Month);  return 0; },
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
    }

    static void AddExpense(string description, decimal amount)
    {
        Functions.Add(description, amount);
    }

    [Verb("list", HelpText = "List all expenses.")]
    public class ListOptions { }

    static void ListExpenses()
    {
        Functions.List();
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
        [Option('m', "month", Required = false, HelpText = "Month number to summarize.")]
        public int Month { get; set; }
    }

    static void GetSummary(int month)
    {
        Functions.Summary(month);
    }
}