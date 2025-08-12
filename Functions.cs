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
        if (!File.Exists(filePath)) File.WriteAllText(filePath, "ID,Date,Description,Amount\n"); // Create file if missing

        return filePath;
    }



    public static void Add(string description, decimal amount)
    {
        try
        {
            string filePath = GetFilePath();
            int ID = GetNextID(filePath);
            string newLine = $"{ID},{DateTime.Today:yyyy-MM-dd},{description},{amount}";

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(newLine);
            }
            Console.WriteLine($"Expense added successfully (ID {ID})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to add expense due to the following error: \n{ex}");
        }
    }

    public static void List()
    {
        try
        {
            if (FileEmpty())
            {
                return;
            }

            List<Expense> expenseList = GetExpenseList();

            // print a header first
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("{0,-7}{1,-15}{2,-30}{3,-15}", "ID", "DATE", "DESCRIPTION", "AMOUNT");
            Console.WriteLine("---------------------------------------------------------------------");

            // print values in the list
            foreach (Expense e in expenseList)
            {
                Console.WriteLine("{0,-7}{1,-15}{2,-30}{3,-15}", e.ID, DateOnly.FromDateTime(e.Date), e.Description, $"$ {e.Amount}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to list expenses due to the following error: \n{ex}");
        }
    }

    public static void Summary(int month = 0)
    {
        try
        {
            if (FileEmpty())
            {
                return;
            }

            List<Expense> expenseList = GetExpenseList();
            decimal total = expenseList.Where(x => x.Date.Month == month || month == 0).Sum(x => x.Amount);

            Console.WriteLine($"Total expenses for {(month == 0 ? "all months" : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month))}: $ {total}");

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

                expenseList.Add(expense);
            }

            return expenseList;
        }
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
