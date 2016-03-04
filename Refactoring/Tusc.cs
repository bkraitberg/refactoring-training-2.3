﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        private static List<User> UserList;
        private static List<Product> ProductList;
        private static User LoggedInUser;
        private static int ProductCount;

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
            string SelectedProductId;
            int QuantityOrdered;

            while (true)
            {
                ShowProductList();
                SelectedProductId = GetValidUserProductSelection();
                if (SelectedProductId.Equals("quit"))
                {
                    UpdateCurrentUsersBalance();
                    break;
                }
                else
                {
                    Product mySelectedProduct = ProductNameLookupById(SelectedProductId);
                    Console.WriteLine();
                    Console.WriteLine("You want to buy: " + mySelectedProduct.Name);
                    Console.WriteLine("Your balance is " + LoggedInUser.Balance.ToString("C"));

                    QuantityOrdered = GetValidUserProductQuantity();
                    if (QuantityOrdered > 0 && VerifyUserFundsForSelectedPurchase(mySelectedProduct, QuantityOrdered) && VerifyStockOnHand(mySelectedProduct, QuantityOrdered))
                    {
                        OrderProduct(mySelectedProduct, QuantityOrdered);
                    }
                    else
                    {
                        ShowPurchaseCancelledMessage();
                    }
                }
            }
        }

        private static Product ProductNameLookupById(string sSelectedProductId)
        {
            //Search for the selected product in our available product
            return ProductList.Where(x => x.Id.Equals(sSelectedProductId)).First();
        }

        private static void ShowPurchaseCancelledMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Purchase cancelled");
            Console.ResetColor();
        }

        private static void OrderProduct(Product SelectedProduct, int QuantityOrdered)
        {
            UpdateBalance(SelectedProduct, QuantityOrdered);
            RemoveItemsFromInventory(SelectedProduct, QuantityOrdered);
            ShowOrderConfirmationMessage(SelectedProduct, QuantityOrdered);
        }

        private static void UpdateBalance(Product SelectedProduct, int QuantityOrdered)
        {
            LoggedInUser.Balance = LoggedInUser.Balance - Convert.ToDouble( SelectedProduct.Price) * QuantityOrdered;
        }

        private static void RemoveItemsFromInventory(Product SelectedProduct, int QuantityOrdered)
        {
            ProductList.Where(x => x.Equals(SelectedProduct)).First().Qty = SelectedProduct.Qty - QuantityOrdered;
           // ProductList[SelectedProduct - 1].Qty = ProductList[SelectedProduct - 1].Qty - QuantityOrdered;
        }

        private static void ShowOrderConfirmationMessage(Product SelectedProduct, int QuantityOrdered)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You bought " + QuantityOrdered + " " + SelectedProduct.Name);
            Console.WriteLine("Your new balance is " + LoggedInUser.Balance.ToString("C"));
            Console.ResetColor();
        }

        private static bool VerifyStockOnHand(Product SelectedProduct, int QuantityOrdered)
        {
            bool stockOnHand = true;
            int iProductCount = SelectedProduct.Qty;

            if ((iProductCount <= 0) || iProductCount < QuantityOrdered)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Sorry, " + SelectedProduct.Name + " is out of stock");
                Console.ResetColor();
                stockOnHand = false;
            }
            return stockOnHand;
        }

        private static bool VerifyUserFundsForSelectedPurchase(Product SelectedProduct, int QuantityOrdered)
        {
            bool fundsAvailable = true;
            if ((LoggedInUser.Balance - (SelectedProduct.Price * QuantityOrdered)) < 0)
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

        private static string GetValidUserProductSelection()
        {
            string productNumber;
            while (true)
            {
                Console.WriteLine("Enter the product Id:");
                string ProductNumberEntered = Console.ReadLine();
                if (validateProduct(ProductNumberEntered, out productNumber))
                {
                    break;
                }
            }
            return productNumber;
        }

        private static bool validateProduct(string ProductNumberEntered, out string productId)
        {
            bool validProductSelected = false;

            //NOTE: productId may consist of Alpha-numerics without special characters
            if (ProductIdEnteredIsValid(ProductNumberEntered, out productId) && ProductIdEnteredExists(productId))
            {
                validProductSelected = true;
            }


            //if (Int32.TryParse(ProductNumberEntered, out productNumber) && (productNumber <= ProductCount + 1))
            //{

            //}
            else if (ProductNumberEntered.ToLower().Equals("quit"))
            {
                productId = ProductNumberEntered.ToLower();
                validProductSelected = true;
            }
            else
            {
                ShowProductNumberInvalidMessage();
            }
            return validProductSelected;
        }

        private static bool ProductIdEnteredIsValid(string sUserSelection, out string sProductIdSelected)
        {
            //remove unwanted characters in the selection.
            sProductIdSelected = Regex.Replace(sUserSelection.Trim(), @"[\\/':*?<>|]", "");

            //Make sure the productSelected is not empty or null
            return !String.IsNullOrWhiteSpace(sProductIdSelected);
        }

        private static bool ProductIdEnteredExists(string sUserSelection)
        {
            bool bFound = false;
            foreach (var prod in ProductList)
            {
                if (prod.Id.Equals(sUserSelection))
                    bFound = true;
            }
            return bFound;
        }

        private static void ShowProductNumberInvalidMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("");
            Console.WriteLine("Product numbers must be numeric in the range of 1 - " + (ProductCount + 1).ToString());
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
                Console.WriteLine(prod.Id + " : " + prod.Name + " (" + prod.Price.ToString("C") + ")");
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

            if (FindUserInUserList(userName, ref user) && ValidateUserPassword(user, userPassword))
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
