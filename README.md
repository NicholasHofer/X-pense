This app was created for the "Expense Tracker" project on Roadmap.sh: https://roadmap.sh/projects/expense-tracker

**X-Pense** is a simple but powerful text-based expense tracking program. 

Commands:

**add** - Add a new expense. {options: --description, --amount}

**list** - List all expenses. {options: (none)}

**delete** - Delete an expense by ID. {options: --id}

**summary** - Get total expenses by month. {options: --month[optional]}

Examples:
add --description "Water bill" --amount 125.87
list
delete --id 3
summary --month 8
summary 
