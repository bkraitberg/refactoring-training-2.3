﻿using Newtonsoft.Json;
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
            string productId = (originalProducts.Find(u => u.Name == "Chips")).Id; 
            
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Program.Main(new string[] { });
                }
            }
        }

        [Test]
        public void Test_TuscDoesNotThrowAnException()
        {
            string productId = (originalProducts.Find(u => u.Name == "Chips")).Id; 
            
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
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

                using (var reader = new StringReader("Joel\r\n"))
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

                using (var reader = new StringReader("\r\n\r\n"))
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

                using (var reader = new StringReader("Jason\r\nsfb\r\n"))
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
            string productId = (originalProducts.Find(u => u.Name == "Chips")).Id;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n0\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
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
            tempUsers.Single(u => u.UserName == "Jason").Balance = 0.0;

            string productId = (originalProducts.Find(u => u.Name == "Chips")).Id;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
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
            tempProducts.Single(u => u.Name == "Chips").Qty = 0;
            string productId = (tempProducts.Find(u => u.Name == "Chips")).Id;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, tempProducts);
                }

                Assert.IsTrue(writer.ToString().Contains("is out of stock"));
            }
        }

        [Test]
        public void Test_UserCanPurchaseProductWhenOnlyOneInStock()
        {
            // Update data file
            List<Product> tempProducts = DeepCopy<List<Product>>(originalProducts);
            tempProducts.Single(u => u.Name == "Chips").Qty = 1;
            string productId = (tempProducts.Find(u => u.Name == "Chips")).Id;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, tempProducts);
                }

                Assert.IsTrue(writer.ToString().Contains("You bought 1 Chips"));
            }
        }

        [Test]
        public void Test_ProductListContainsExitItem()
        {
            string productId = (originalProducts.Find(u => u.Name == "Chips")).Id; 

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains(Tusc.QuitCommand));
            }
        }

        [Test]
        public void Test_UserCanExitByEnteringQuit()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\nquit\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, products);
                }

                Assert.IsTrue(writer.ToString().Contains("Type quit to exit the application"));
                Assert.IsTrue(writer.ToString().Contains("Press Enter key to exit"));
            }
        }

        [Test]
        public void Test_ProductsWithZeroQuantityDoNotAppearInMenu()
        {
            // Update data file
            List<Product> tempProducts = DeepCopy<List<Product>>(originalProducts);
            tempProducts.Single(u => u.Name == "Chips").Qty = 0;
            string productId = (tempProducts.Find(u => u.Name == "Chips")).Id;

            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                using (var reader = new StringReader("Jason\r\nsfa\r\n" + productId + "\r\n1\r\n" + Tusc.QuitCommand + "\r\n\r\n"))
                {
                    Console.SetIn(reader);

                    Tusc.Start(users, tempProducts);
                }

                // Requirement is only to remove chips from display, not prevent the user from selecting a hidden ID.
                // Hidden IDs can still be seleted but will result in the 'out of stock' message.
                Assert.IsTrue(writer.ToString().Contains("Chips is out of stock")); 
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
