using System.Globalization;
using static X_pense.Models;

namespace X_pense;

public class Functions
{
    private static string GetFilePath()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Data");
        string filePath = Path.Combine(directory, "Expenses.csv");


        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory); // Create folder if missing
        if (!File.Exists(filePath)) File.WriteAllText(filePath, "ID,Date,Description,Amount,Category\n"); // Create expense file if missing


        return filePath;
    }

    private static string GetBudgetFilePath()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Data");
        string filePathBudget = Path.Combine(directory, "Budget.csv");

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory); // Create folder if missing
        if (!File.Exists(filePathBudget)) File.WriteAllText(filePathBudget, "Month,Amount\n"); // Create budget file if missing

        return filePathBudget;
    }


    public static void Add(string description, decimal amount, string category)
    {
        try
        {
            string filePath = GetFilePath();
            int ID = GetNextID(filePath);
            string newLine = $"{ID},{DateTime.Today:yyyy-MM-dd},{description},{amount},{category}";

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(newLine);
            }
            Console.WriteLine($"Expense added successfully (ID {ID})");

            // Check budget for this month
            var budgetList = GetBudgetList();
            decimal monthlyBudget = budgetList.Where(x => x.Month == DateTime.Today.Month).Select(x => x.Amount).FirstOrDefault();
            var expenseList = GetExpenseList();
            decimal currentMonthlyTotal = expenseList.Where(x => x.Date.Month == DateTime.Today.Month).Select(x => x.Amount).Sum();

            Console.WriteLine($"Monthly total for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month)}: ${currentMonthlyTotal}");
            Console.WriteLine($"Monthly budget for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month)}: ${monthlyBudget}");

            decimal pctUsed = currentMonthlyTotal / monthlyBudget * 100;
            Console.WriteLine($"Total percentage of budget used: {Convert.ToInt16(pctUsed)}%");
            if (Convert.ToInt16(pctUsed) > 100)
            {
                Console.WriteLine("WARNING: MONTHLY BUDGET EXCEEDED");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to add expense due to the following error: \n{ex}");
        }
    }

    public static void SetBudget(int month, decimal amount)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                Console.WriteLine("Month not valid. Must be a number between 1 and 12.");
                return;
            }

            // Load existing budget list from file
            var budgetList = GetBudgetList();

            // Set budget for specified month
            var existingBudget = budgetList.FirstOrDefault(x => x.Month == month);

            if (existingBudget != null) // if a budget already exists for a month, update it
            {
                existingBudget.Amount = amount;
                Console.WriteLine($"Updated budget for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)}: {amount:C}");
            }
            else // if budget doesn't exist for month, add it
            {
                budgetList.Add(new Budget { Month = month, Amount = amount });
                Console.WriteLine($"Added budget for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)}: {amount:C}");
            }

            // Save changes to csv file
            var lines = new List<string> { "Month,Amount" }; //header
            lines.AddRange(budgetList.Select(x => $"{x.Month},{x.Amount}"));
            File.WriteAllLines(GetBudgetFilePath(), lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to set budget:\n{ex}");
        }
    }

    public static void List(string? category = null)
    {
        try
        {
            if (FileEmpty())
            {
                return;
            }

            List<Expense> expenseList = GetExpenseList();
            var filteredExpenseList = expenseList.Where(x => category is null || x.Category == category);

            // print a header first
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-7}{1,-15}{2,-30}{3,-15}{4,-25}", "ID", "DATE", "DESCRIPTION", "AMOUNT", "CATEGORY");
            Console.WriteLine("------------------------------------------------------------------------------------------");

            // print values in the list
            foreach (Expense e in filteredExpenseList)
            {
                Console.WriteLine("{0,-7}{1,-15}{2,-30}{3,-15}{4,-25}", e.ID, DateOnly.FromDateTime(e.Date), e.Description, $"$ {e.Amount}", e.Category);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to list expenses due to the following error: \n{ex}");
        }
    }

    public static void Summary(string? category, int month = 0)
    {
        try
        {
            if (FileEmpty())
            {
                return;
            }

            List<Expense> expenseList = GetExpenseList();
            decimal total = expenseList.Where(x => (x.Date.Month == month || month == 0) && (category is null || x.Category == category)).Sum(x => x.Amount);

            Console.WriteLine($"Total {(category is null ? "of all" : category)} expenses for {(month == 0 ? "all months" : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month))}: $ {total}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to list summary due to the following error: \n{ex}");
        }
    }

    public static void Delete(int id)
    {
        try
        {
            if (FileEmpty())
            {
                return;
            }

            List<Expense> expenseList = GetExpenseList();
            string filePath = GetFilePath();

            // get record for specified ID
            var expenseToRemove = expenseList.FirstOrDefault(x => x.ID == id);

            // remove expense from the list, if exists
            if (expenseToRemove != null)
            {
                expenseList.Remove(expenseToRemove);
            }
            else
            {
                Console.WriteLine($"Expense ID {id} not found");
                return;
            }

            // write the updated list back to the file
            using (StreamWriter writer = new StreamWriter(filePath, false)) // false parameter means the file will be overwritten, not appended to
            {
                foreach (Expense e in expenseList)
                {
                    writer.WriteLine($"{e.ID},{DateOnly.FromDateTime(e.Date)},{e.Description},{e.Amount}");
                }
            }

            Console.WriteLine($"Expense ID {id} has been deleted");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to delete expense due to the following error: \n{ex}");
        }
    }

    private static int GetNextID(string filePath)
    {
        if (FileEmpty())
        {
            return 1;
        }
        else
        {
            // get max(ID) from .csv file and increment it by 1 
            var lines = File.ReadAllLines(filePath).Skip(1) // skip header row
                                                   .Where(line => !string.IsNullOrWhiteSpace(line)) // skip empty lines
                                                   .Where(line => int.TryParse(line.Split(',')[0], out _)); // only lines with valid int ID
            int ID = lines.Select(line => int.Parse(line.Split(',')[0])).Max();
            ID += 1;

            return ID;
        }
    }

    private static List<Expense> GetExpenseList()
    {
        string filePath = GetFilePath();
        using (StreamReader reader = new StreamReader(filePath))
        {
            // read lines from .csv file and load them into a list of Expenses
            List<Expense> expenseList = new List<Expense>();
            bool isFirstLine = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    continue;
                }

                // skip header row
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var values = line.Split(',');

                Expense expense = new Expense();
                expense.ID = Convert.ToInt32(values[0]);
                expense.Date = Convert.ToDateTime(values[1]);
                expense.Description = values[2];
                expense.Amount = Convert.ToDecimal(values[3]);
                expense.Category = values[4];

                expenseList.Add(expense);
            }

            return expenseList;
        }
    }

    private static List<Budget> GetBudgetList()
    {
        string filePathBudget = GetBudgetFilePath();
        var budgetList = new List<Budget>();

        using (StreamReader reader = new StreamReader(filePathBudget))
        {
            bool isFirstLine = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    continue;
                }

                // skip header row
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var values = line.Split(',');
                budgetList.Add(new Budget { Month = int.Parse(values[0]), Amount = Convert.ToDecimal(values[1]) });
            }
        }

        return budgetList;
    }

    private static bool FileEmpty()
    {
        string filePath = GetFilePath();
        var lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1)
        {
            Console.WriteLine("CSV file is empty. Please add expenses.");
            return true;
        }
        else
        {
            return false;
        }
    }
}
