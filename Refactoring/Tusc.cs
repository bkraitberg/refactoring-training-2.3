﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        private static List<User> UserList;
        private static List<Product> ProductList;
        private static User LoggedInUser;
        private static int ProductCount;
        private const string EXIT_COMMAND = "quit";

        public static void Start(List<User> users, List<Product> products)
        {
            InitializeMemberVariables(users, products);
            ShowWelcomeMessage();
            if (LoginUser())
            {
                ShowRemainingBalance();
                OrderProducts();
            }
            ShowCloseApplicationMessage();
        }

        private static void ShowCloseApplicationMessage()
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }

        private static void InitializeMemberVariables(List<User> usrs, List<Product> prods)
        {
            UserList = usrs;
            ProductList = prods;
            ProductCount = prods.Count;
        }

        private static void OrderProducts()
        {
            string productId = string.Empty;
            int QuantityOrdered;
            Product prod;

            while (true)
            {
                ShowProductList();
                if (GetProductSelection(out productId, out prod))
                {
                    Console.WriteLine();
                    Console.WriteLine("You want to buy: " + prod.Name);
                    Console.WriteLine("Your balance is " + LoggedInUser.Balance.ToString("C"));

                    QuantityOrdered = GetValidUserProductQuantity();
                    if (QuantityOrdered > 0 && VerifyUserFundsForSelectedPurchase(prod, QuantityOrdered) && VerifyStockOnHand(prod, QuantityOrdered))
                    {
                        OrderProduct(prod, QuantityOrdered);
                    }
                    else
                    {
                        ShowPurchaseCancelledMessage();
                    }
                }
                else
                {
                    if (IsStringExitCommand(productId))
                    {
                        UpdateCurrentUsersBalance();
                        break;
                    }
                }
            }
        }

        private static bool IsStringExitCommand(string productId)
        {
            bool userQuit = false;
            if (productId == EXIT_COMMAND)
            {
                userQuit = true;
            }
            return userQuit;
        }

        private static void ShowPurchaseCancelledMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Purchase cancelled");
            Console.ResetColor();
        }

        private static void OrderProduct(Product prod, int QuantityOrdered)
        {
            UpdateBalance(prod, QuantityOrdered);
            RemoveItemsFromInventory(prod, QuantityOrdered);
            ShowOrderConfirmationMessage(prod, QuantityOrdered);
        }

        private static void UpdateBalance(Product prod, int QuantityOrdered)
        {
            LoggedInUser.Balance =  LoggedInUser.Balance - (prod.Price * QuantityOrdered);
        }

        private static void RemoveItemsFromInventory(Product prod, int QuantityOrdered)
        {
            prod.Qty = prod.Qty - QuantityOrdered;
        }

        private static void ShowOrderConfirmationMessage(Product prod, int QuantityOrdered)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You bought " + QuantityOrdered + " " + prod.Name);
            Console.WriteLine("Your new balance is " + LoggedInUser.Balance.ToString("C"));
            Console.ResetColor();
        }

        private static bool VerifyStockOnHand(Product prod, int QuantityOrdered)
        {
            bool stockOnHand = true;
            if (prod.Qty < QuantityOrdered)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Sorry, " + prod.Name + " is out of stock");
                Console.ResetColor();
                stockOnHand = false;
            }
            return stockOnHand;
        }

        private static bool VerifyUserFundsForSelectedPurchase(Product prod, int QuantityOrdered)
        {
            bool fundsAvailable = true;
            if ((LoggedInUser.Balance - (prod.Price * QuantityOrdered)) < 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("You do not have enough money to buy that.");
                Console.ResetColor();
                fundsAvailable = false;
            }
            return fundsAvailable;
        }

        private static int GetValidUserProductQuantity()
        {
            int Quantity = 0;
            while (true)
            {
                Console.WriteLine("Enter quantity to purchase:");
                string QuantityEntered = Console.ReadLine();
                if (ValidateQuantityEntered(QuantityEntered, out Quantity))
                {
                    break;
                }
            }
            return Quantity;
        }

        private static bool ValidateQuantityEntered(string quantityEntered, out int Quantity)
        {
            bool ValidQuantitySelected = false;
            if (ConvertStringToInteger(quantityEntered, out Quantity) && (Quantity >= 0))
            {
                ValidQuantitySelected = true;
            }
            else
            {
                ShowInvalidQuantitySelectedMessage();
            }
            return ValidQuantitySelected;
        }

        private static void ShowInvalidQuantitySelectedMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("");
            Console.WriteLine("Selected quantity must be numeric and greater than 0");
            Console.WriteLine("");
            Console.ResetColor();
        }

        private static bool ConvertStringToInteger(string input, out int stringAsInteger)
        {
            return Int32.TryParse(input, out stringAsInteger);
        }

        private static void UpdateCurrentUsersBalance()
        {
            string json = JsonConvert.SerializeObject(UserList, Formatting.Indented);
            File.WriteAllText(@"Data/Users.json", json);

            string json2 = JsonConvert.SerializeObject(ProductList, Formatting.Indented);
            File.WriteAllText(@"Data/Products.json", json2);
        }

        private static bool GetProductSelection(out string productId, out Product prod )
        {
            bool validProductSelected = true;
            prod = null;
            
            productId = PromptUserForProduct();
            if (!IsProductInProductList(productId, out prod))
            {
                validProductSelected = false;
                if (!IsStringExitCommand(productId))
                {
                    ShowProductNumberInvalidMessage();
                }
            }
           
            return validProductSelected;
        }

        private static string PromptUserForProduct()
        {
            Console.WriteLine("Enter the Product Id:");
            string ProductId = Console.ReadLine();
            return ProductId;
        }

        private static bool IsProductInProductList(string productId, out Product prod )
        {
            bool validProductSelected = false;

            prod = ProductList.Find(p => p.Id.Equals(productId));
            validProductSelected = prod != null;
            return validProductSelected;
        }

        private static void ShowProductNumberInvalidMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("");
            Console.WriteLine("Product numbers must be numeric in the range of 1 - " + ProductCount.ToString());
            Console.WriteLine("");
            Console.ResetColor();
        }

        private static void ShowProductList()
        {
            Console.WriteLine();
            Console.WriteLine("What would you like to buy?");
            for (int i = 0; i < ProductCount; i++)
            {
                Product prod = ProductList[i];
                if (prod.Qty >0)
                    Console.WriteLine("ProductId: " + prod.Id + " Product Name: " + prod.Name + " (" + prod.Price.ToString("C") + ")");
            }
            Console.WriteLine("Type quit to exit the application");
        }

        private static void ShowRemainingBalance()
        {
            Console.WriteLine();
            Console.WriteLine("Your balance is " + LoggedInUser.Balance.ToString("C"));
            return;
        }

        private static void ShowSuccessfulLoginMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Login successful! Welcome " + LoggedInUser.UserName + "!");
            Console.ResetColor();
        }

        private static bool LoginUser()
        {
            bool validatedUser = false;
            string userName = string.Empty;
            string userPassword = string.Empty;
            User user = new User();

            GetUserCredentials(ref userName, ref userPassword);
            if (ValidateUserCredentials(userName, userPassword, ref user)) 
            {
                LoggedInUser = user;
                ShowSuccessfulLoginMessage();
                validatedUser = true;
            }
            else
            {
                ShowFailedCredentialsMessage();
            }
            return validatedUser;
         }

        private static void GetUserCredentials(ref string userName, ref string userPassword)
        {
            userName = GetUserLogin();
            userPassword = GetUserPassword();
        }

        private static void ShowFailedCredentialsMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("You entered an invalid userid or password.");
            Console.WriteLine("The TUSC application will now close.");
            Console.ResetColor();
        }

        private static bool ValidateUserPassword(User userName, string password)
        {
            bool passwordValid = false;
            if (userName.Password == password)
            { 
                passwordValid = true;
            }
            return passwordValid;
        }

        private static string GetUserLogin()
        {
            Console.WriteLine();
            Console.WriteLine("Enter Username:");
            return Console.ReadLine();
        }

        private static string GetUserPassword()
        {
            Console.WriteLine("Enter Password:");
            return Console.ReadLine();
        }

        private static bool ValidateUserCredentials(string userName, string userPassword, ref User user)
        {
            bool validCredentials = false;

            if(FindUserInUserList(userName, ref user) && ValidateUserPassword(user, userPassword))
            {
                validCredentials = true;
            }
            return validCredentials;
        }

        private static bool FindUserInUserList(string name, ref User foundUser)
        {
            bool UserIsFound = false;
            if (!string.IsNullOrEmpty(name))
            {
                foreach (User user in UserList)
                {
                    if (user.UserName == name)
                    {
                        UserIsFound = true;
                        foundUser = user;
                        break;
                    }
                }
            }
            return UserIsFound;
        }

        private static void ShowWelcomeMessage()
        {
            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");
        }
    }
}
