namespace X_pense;

public class Models
{
    public class Expense
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }

        public Expense()
        {
            
        }
    }
}
