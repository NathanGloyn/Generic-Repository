using System;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_inserting_data
    {
        [TearDown]
        public void ResetData()
        {
            DBHelper.Execute(@"..\..\TestScripts\Setup\04_Reset_Data.sql");
        }

        [Test]
        public void Should_throw_ArgumentNullException_if_null_entity_provided()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                Assert.Throws<ArgumentNullException>(() => repo.Insert(null));
            }            
        }

        [Test]
        public void Should_insert_new_record_and_populate_Id()
        {
            var orderDate = DateTime.Now.AddDays(-20);
            var requiredDate = DateTime.Now.AddDays(-10);
            var shippedDate = DateTime.Now.AddDays(-15);

            var newOrder = new Order
                               {
                                   CustomerID = "ABCDE",
                                   EmployeeID = 5,
                                   Freight = 12.13m,
                                   OrderDate = orderDate,
                                   RequiredDate = requiredDate,
                                   ShipName = "My Stuff",
                                   ShipAddress = "P.O. Box 123",
                                   ShipCity = "London",
                                   ShipPostalCode = "EC1 4RT",
                                   ShipCountry = "UK",
                                   ShipVia = 3,
                                   ShippedDate = shippedDate
                               };
            
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                repo.Insert(newOrder);
            }

            Assert.AreEqual(10348, newOrder.OrderID);

        }

        [Test]
        public void Should_insert_record_with_nulls_where_appropriate()
        {
            var orderDate = DateTime.Now.AddDays(-20);
            var requiredDate = DateTime.Now.AddDays(-10);
            var shippedDate = DateTime.Now.AddDays(-15);

            var newOrder = new Order
            {
                CustomerID = "ABCDE",
                Freight = 12.13m,
                OrderDate = orderDate,
                RequiredDate = requiredDate,
                ShipName = "My Stuff",
                ShipAddress = "P.O. Box 123",
                ShipCity = "London",
                ShipPostalCode = "EC1 4RT",
                ShipCountry = "UK",
                ShipVia = 3,
                ShippedDate = shippedDate
            };

            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                repo.Insert(newOrder);
            }

            Order inserted = null;

            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                inserted = repo.GetById(newOrder.OrderID);
            }

            Assert.IsNull(inserted.EmployeeID);
            Assert.IsNull(inserted.ShipRegion);
        }

    }
}