using Newtonsoft.Json;
using NUnit.Framework;
using Refactoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitTestProject
{
    [TestFixture]
    public class UnitTests
    {
        private List<User> users;
        private List<User> originalUsers;
        private List<Product> products;
        private List<Product> originalProducts;

        private const string EXIT_TEXT = "quit";
        private const string MENU_OPTION_CHIPS = "c1";
        private const string PRESS_ENTER = "\r\n";
        private const string JASON_LOGIN_ENTRY = "Jason" + PRESS_ENTER + "sfa" + PRESS_ENTER;


        [SetUp]
        public void Test_Initialize()
        {
            // Load users from data file
            originalUsers = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(@"Data/Users.json"));
            users = DeepCopy<List<User>>(originalUsers);

            // Load products from data file
            originalProducts = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText(@"Data/Products.json"));
            products = DeepCopy<List<Product>>(originalProducts);
        }

        [TearDown]
        public void Test_Cleanup()
        {
            // Restore users
            string json = JsonConvert.SerializeObject(originalUsers, Formatting.Indented);
            File.WriteAllText(@"Data/Users.json", json);
            users = DeepCopy<List<User>>(originalUsers);

            // Restore products
            string json2 = JsonConvert.SerializeObject(originalProducts, Formatting.Indented);
            File.WriteAllText(@"Data/Products.json", json2);
            products = DeepCopy<List<Product>>(originalProducts);
        }

        [Test]
        public void Test_StartingTuscFromMainDoesNotThrowAnException()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    MENU_OPTION_CHIPS +
                    PRESS_ENTER +
                    "1" +
                    PRESS_ENTER +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Program.Main(new string[] { });
                }
            }
        }

        [Test]
        public void Test_TuscDoesNotThrowAnException()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    MENU_OPTION_CHIPS +
                    PRESS_ENTER +
                    "1" +
                    PRESS_ENTER +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }
            }
        }

        [Test]
        public void Test_InvalidUserIsNotAccepted()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    "Joel" +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains("You entered an invalid user"));
            }
        }

        [Test]
        public void Test_EmptyUserDoesNotThrowAnException()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }
            }
        }

        [Test]
        public void Test_InvalidPasswordIsNotAccepted()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason" + PRESS_ENTER + "sfb" + PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains("You entered an invalid userid or password"));
            }
        }

        [Test]
        public void Test_UserCanCancelPurchase()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    MENU_OPTION_CHIPS +
                    PRESS_ENTER +
                    "0" +
                    PRESS_ENTER +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains("Purchase cancelled"));

            }
        }

        [Test]
        public void Test_ErrorOccursWhenBalanceLessThanPrice()
        {
            // Update data file
            List<User> tempUsers = DeepCopy<List<User>>(originalUsers);
            tempUsers.Where(u => u.UserName == "Jason").Single().Balance = 0.0;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    MENU_OPTION_CHIPS +
                    PRESS_ENTER +
                    "1" +
                    PRESS_ENTER +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(tempUsers, products);
                }

                Assert.IsTrue(writer.ToString().Contains("You do not have enough money to buy that"));
            }
        }

        [Test]
        public void Test_ErrorOccursWhenProductOutOfStock()
        {
            // Update data file
            List<Product> tempProducts = DeepCopy<List<Product>>(originalProducts);
            tempProducts.Where(u => u.Name == "Chips").Single().Qty = 0;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    MENU_OPTION_CHIPS +
                    PRESS_ENTER +
                    "1" +
                    PRESS_ENTER +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, tempProducts);
                }

                Assert.IsTrue(writer.ToString().Contains("is out of stock"));
            }
        }

        [Test]
        public void Test_MenuListContainsExitOption()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(JASON_LOGIN_ENTRY + EXIT_TEXT + PRESS_ENTER + PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains("Type quit to exit the application"));
            }
        }

        [Test]
        public void Test_UserCanPurchaseProductWhenOnlyOneInStock()
        {
            // Update data file
            List<Product> tempProducts = DeepCopy<List<Product>>(originalProducts);
            tempProducts.Where(u => u.Name == "Chips").Single().Qty = 1;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    MENU_OPTION_CHIPS +
                    PRESS_ENTER +
                    "1" +
                    PRESS_ENTER +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, tempProducts);
                }

                Assert.IsTrue(writer.ToString().Contains("You bought 1 Chips"));
            }
        }

        [Test]
        public void Test_UserCanExitByEnteringQuit()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader(
                    JASON_LOGIN_ENTRY +
                    EXIT_TEXT +
                    PRESS_ENTER +
                    PRESS_ENTER))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains("Type quit to exit the application"));
                Assert.IsTrue(writer.ToString().Contains("Press Enter key to exit"));
            }
        }

        private static T DeepCopy<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
