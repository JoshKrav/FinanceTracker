﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Windows;

// Implements the INotifyPropertyChanged interface.
// This interface is used to make changes and provide notifications when the value
// of a property has changed.
public class Finances : INotifyPropertyChanged
{
    private decimal _balance;
    private decimal _expenses;
    private decimal _income;
    private int recordCount;
    private List<FinancialRecords> _records = new List<FinancialRecords> { };

    // Create an event that will update the WPF document with the totals from the class.
    public event PropertyChangedEventHandler PropertyChanged;

    // The method takes a string parameter of the property that changed. 
    // If it's null, it will do nothing. If there's something, it will call
    // Invoke() which triggers the event.
    void OnPropertyChanged(string propertyName)
    {
        // "this" refers to the current instance of the Finances class (though
        // there will only be one). PropertyChangedEventArgs is a class that
        // provides data for the PropertyChanged event.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Constructor
    public Finances()
    {
        InitializeBalance();

        InitializeFinancialRecords();
    }

    private void InitializeBalance()
    {
        string incomeFilePath = "income.txt";
        if (File.Exists(incomeFilePath))
        {
            // Read the existing balance from the file
            string balanceStr = File.ReadAllText(incomeFilePath);
            decimal.TryParse(balanceStr, out _balance);
            _balance = Math.Round(_balance, 2);
        }
        else
        {
            // File doesn't exist, so create it and initialize balance to 0
            _balance = 0;
            File.WriteAllText(incomeFilePath, _balance.ToString());
        }
    }

    private void InitializeFinancialRecords()
    {
        _expenses = 0m;

        string[] csvFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*_expenses.csv");
        foreach (string file in csvFiles)
        {
            string[] lines = File.ReadAllLines(file);
            bool isFirstLine = true; // Flag to skip the header line

            foreach (string line in lines)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue; // Skip the header line
                }

                string[] parts = line.Split(',');
                if (parts.Length == 6)
                {
                    decimal.TryParse(parts[2], out decimal expense);
                    decimal.TryParse(parts[5], out decimal amountPayed);
                    int.TryParse(parts[4], out int year);

                    FinancialRecords record = new FinancialRecords(expense, parts[1], parts[3], year)
                    {
                        ID = int.Parse(parts[0]),
                        AmountPayed = amountPayed
                    };

                    _records.Add(record);
                    _expenses += expense;
                }
            }
        }
    }
    #endregion


    public decimal Balance
    {
        get
        {
            return _balance;
        }
        set
        {
            // No need for validation as value can be a negative as they can be in debt.
            _balance = value;
            OnPropertyChanged(nameof(Balance)); // Notify that balance has been changed.
        }
    }

    public List<FinancialRecords> GetFinancialRecords()
    {
        return _records;
    }
    public void SetFinancialRecord(FinancialRecords record)
    {      
            Expenses += record.Expense;
            record.ID = _records.Count;
            _records.Add(record);     
        
    }
    public decimal Expenses
    {
        get
        {
            return _expenses;
        }
        private set
        {
            _expenses = value;
            OnPropertyChanged(nameof(Expenses)); // Notify that expenses has been changed.
        }
    }

    public decimal Income
    {
        get
        {
            return _income;
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Income cannot be less than 0.");
            }
            _income = value;
        }
    }

    public decimal AddIncome(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Added income cannot be a negative.");
        }
        amount = Math.Round(amount, 2);
        Income += amount;
        Balance += amount;
        return Balance;
    }

    public decimal PayExpense(int id, decimal amount)
    {
        if (_records[id].Expense - amount < 0)
        {
            MessageBox.Show("Amount exceeds the expense.");
        }
        else if (Balance - amount < 0)
        {
            MessageBox.Show("You will go into debt if you pay off this expense!");
        }
        else
        {
            _records[id].IncomeSpent += amount;
            Balance -= amount;
            Expenses -= amount;
            _records[id].Expense -= amount;
            SubtractIncome(amount);
        }
        
        return Balance;
    }

    public void AddExpense(int id, decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be less than 0.");
        }
        Expenses += amount;
        _records[id].Expense += amount;
    }
    private void SubtractIncome(decimal amount)
    {
        string incomeFilePath = "income.txt";
        if (File.Exists(incomeFilePath))
        {
            // Read the existing balance from the file
            string balanceStr = File.ReadAllText(incomeFilePath);
            decimal.TryParse(balanceStr, out _balance);
            _balance = Math.Round(_balance, 2);
            _balance -= Math.Round(amount,2);
            File.WriteAllText(incomeFilePath, _balance.ToString());
        }
        
    }
    public void DeleteRecord(FinancialRecords record)
    {
        _records.Remove(record);      
    }


}
