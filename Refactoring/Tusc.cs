using Newtonsoft.Json;
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
        private static List<Product> DisplayProductList;
        private static User LoggedInUser;
        
        private const int QUIT_FLAG = -1;
        private const string QUIT_STRING_LOWERCASE = "quit";

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
            DisplayProductList = prods.Where(t => t.Qty > 0).ToList();
        }

        private static int ProductCount
        {
            get
            { 
                return DisplayProductList.Count; 
            } 
        }

        private static void OrderProducts()
        {
            int SelectedProductNumber;
            Product SelectedProduct;
            int QuantityOrdered;

            while (true)
            {
                if (ProductCount > 0)
                {
                    ShowProductList();
                    SelectedProductNumber = GetValidUserProductSelection();
                    if (SelectedProductNumber == QUIT_FLAG)
                    {
                        UpdateCurrentUsersBalance();
                        break;
                    }
                    else
                    {
                        SelectedProduct = ProductList.Single(t => t.Name == DisplayProductList[SelectedProductNumber - 1].Name);

                        Console.WriteLine();
                        Console.WriteLine("You want to buy: " + SelectedProduct.Name);
                        Console.WriteLine("Your balance is " + LoggedInUser.Balance.ToString("C"));

                        QuantityOrdered = GetValidUserProductQuantity();
                        if (QuantityOrdered > 0 && VerifyUserFundsForSelectedPurchase(SelectedProduct, QuantityOrdered) && VerifyStockOnHand(SelectedProduct, QuantityOrdered))
                        {
                            OrderProduct(SelectedProduct, QuantityOrdered);
                        }
                        else
                        {
                            ShowPurchaseCancelledMessage();
                        }
                    }
                }
                else
                {
                    ShowEmptyStoreMessage();
                    break;
                }
            }
        }

        private static void ShowEmptyStoreMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("TUSC is completely out of stock. Nag Dave and Jason until they buy more stuff.");
            Console.ResetColor();
        }

        private static void ShowPurchaseCancelledMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Purchase cancelled");
            Console.ResetColor();
        }

        private static void OrderProduct(Product product, int QuantityOrdered)
        {
            UpdateBalance(product, QuantityOrdered);
            RemoveItemsFromInventory(product, QuantityOrdered);
            ShowOrderConfirmationMessage(product, QuantityOrdered);
        }

        private static void UpdateBalance(Product selectedProduct, int QuantityOrdered)
        {
            LoggedInUser.Balance -= (selectedProduct.Price * QuantityOrdered);
        }

        private static void RemoveItemsFromInventory(Product SelectedProduct, int QuantityOrdered)
        {
            SelectedProduct.Qty -= QuantityOrdered;
            if(SelectedProduct.Qty == 0)
            {
                DisplayProductList.Remove(SelectedProduct);
            }
        }

        private static void ShowOrderConfirmationMessage(Product SelectedProduct, int QuantityOrdered)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You bought " + QuantityOrdered + " " + SelectedProduct.Name);
            Console.WriteLine("Your new balance is " + LoggedInUser.Balance.ToString("C"));
            Console.ResetColor();
        }

        private static bool VerifyStockOnHand(Product selectedProduct, int QuantityOrdered)
        {
            bool stockOnHand = true;
            if (selectedProduct.Qty < QuantityOrdered)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Sorry, " + selectedProduct.Name + " is out of stock");
                Console.ResetColor();
                stockOnHand = false;
            }
            return stockOnHand;
        }

        private static bool VerifyUserFundsForSelectedPurchase(Product product, int QuantityOrdered)
        {
            bool fundsAvailable = true;
            if ((LoggedInUser.Balance - (product.Price * QuantityOrdered)) < 0)
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

        private static int GetValidUserProductSelection()
        {
            int productNumber;
            while (true)
            {
                Console.WriteLine("Enter the product number:");
                string ProductNumberEntered = Console.ReadLine();
                if (validateProduct(ProductNumberEntered, out productNumber))
                {
                    break;
                }
            }
            return productNumber;
        }

        private static bool validateProduct(string ProductNumberEntered, out int productNumber )
        {
            bool validProductSelected = false;

            if(QUIT_STRING_LOWERCASE.Equals(ProductNumberEntered.ToLowerInvariant()))
            {
                productNumber = QUIT_FLAG;
                validProductSelected = true;
            }
            else if (Int32.TryParse(ProductNumberEntered, out productNumber) && (productNumber <= ProductCount))
            {
                validProductSelected = true;
            }
            else
            {
                ShowProductNumberInvalidMessage();
            }
            return validProductSelected;
        }

        private static void ShowProductNumberInvalidMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("");
            Console.WriteLine("Product numbers must be numeric in the range of 1 - " + (ProductCount).ToString());
            Console.WriteLine("");
            Console.ResetColor();
        }

        private static void ShowProductList()
        {
            Console.WriteLine();
            Console.WriteLine("What would you like to buy? Type quit to exit the application");
            for (int i = 0; i < ProductCount; i++)
            {
                Product prod = DisplayProductList[i];
                Console.WriteLine(i + 1 + ": " + prod.Name + " (" + prod.Price.ToString("C") + ")");
            }
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
