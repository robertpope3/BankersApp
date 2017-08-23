using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankersApp.DBML;

namespace BankersApp
{
    class Helper
    {
        private int AddCustomer(string firstName, string lastName, string address1, string address2, string city, string state, string zipCode, string phone1, string country)
        {
            int customerID = 0;
            using (DataClasses1DataContext dbml = new DataClasses1DataContext())
            {
                try
                {
                    Customer cust = new Customer();
                    cust.FirstName = firstName;
                    cust.LastName = lastName;
                    cust.Address1 = address1;
                    cust.Address2 = address2;
                    cust.City = city;
                    cust.State = state;
                    cust.ZipCode = zipCode;
                    cust.Phone1 = phone1;
                    cust.Country = country;

                    dbml.Customers.InsertOnSubmit(cust);
                    dbml.SubmitChanges();
                }
                catch
                {
                    customerID = -1;
                    // future- error code for customer problems
                }
                return customerID;
            }
        }
        private int AddAccount(int acctType, int customerID, decimal balance = 0)
        {
            int accountID = 0;

            using (DataClasses1DataContext dbml = new DataClasses1DataContext())
            {
                try
                {
                    Account acct = new Account();
                    acct.AccountType = acctType;
                    acct.Balance = balance;

                    dbml.Accounts.InsertOnSubmit(acct);
                    dbml.SubmitChanges();

                    accountID = acct.AccountID;

                    CustomersInAccount cia = new CustomersInAccount();
                    cia.CustomerID = customerID;
                    cia.AccountID = accountID;

                    dbml.CustomersInAccounts.InsertOnSubmit(cia);
                    dbml.SubmitChanges();
                }
                catch
                {
                    accountID = -1;
                    // future- error code for account problems
                }
                return accountID;
            }
        }
        private int AddTransaction(decimal amount, int accountID, int toAccountID = 0)
        {
            int transactionID;
            using (DataClasses1DataContext dbml = new DataClasses1DataContext())
            {
                Account acct = dbml.Accounts.Single(a => a.AccountID == accountID);
                decimal balance = acct.Balance;
                AccountType acctType = dbml.AccountTypes.Single(t => t.AccountTypeID == acct.AccountType);
                
                if (amount < acctType.TransactionLimit)
                {
                    transactionID = -2; // Transaction Limit
                    return transactionID;
                }

                if (balance + amount < 0)
                {
                    transactionID = -1; // insufficient funds
                    return transactionID;
                }

                try
                {
                    Transaction trans = new Transaction();
                    trans.AccountID = accountID;
                    trans.Amount = amount;

                    dbml.Transactions.InsertOnSubmit(trans);
                    dbml.SubmitChanges(); // Makes sure the transaction is complete before 

                    acct.Balance = balance + amount;
                    dbml.SubmitChanges();

                    transactionID = trans.TransactionID;
                    return transactionID;
                }
                catch
                {
                    transactionID = -3; // sql/connection error
                    return transactionID;
                }
                
                // if toAccount provided will create absolute transaction amount in receiving account.
                if (toAccountID != 0)
                {
                    Account toAcct = dbml.Accounts.Single(x => x.AccountID == toAccountID);

                    amount = Math.Abs(amount);
                    AddTransaction(amount, toAcct.AccountID);
                }
            }
        }
        private int AddCustomerToAccount(int accountID, int customerID)
        {
            int ciaID;
            using (DataClasses1DataContext dbml = new DataClasses1DataContext())
            {
                CustomersInAccount cia = new CustomersInAccount();
                cia.CustomerID = customerID;
                cia.AccountID = accountID;

                dbml.CustomersInAccounts.InsertOnSubmit(cia);
                dbml.SubmitChanges();
                ciaID = cia.CustomersInAccountID;
            }
            return ciaID;
        }
    }
}
