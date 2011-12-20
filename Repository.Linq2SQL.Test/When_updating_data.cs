using System;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_updating_data
    {
        [Test]
        public void Should_throw_exception_if_null_passed_in()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                Assert.Throws<ArgumentNullException>(() => repo.Update(null));
            }
        }

        [Test]
        public void Should_update_single_field_entity()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);
                }

                // alter property and then update -- Act
                toUpdate.ShipName = "abc";

                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.AreEqual("abc", original.ShipName);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                DBHelper.Execute(@"..\..\TestScripts\Reset_Update_ShipName.sql");
            }
        }        

        [Test]
        public void Should_update_null_property_to_given_value()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);
                }

                toUpdate.ShipRegion = "West";

                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.AreEqual("West", original.ShipRegion);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                DBHelper.Execute(@"..\..\TestScripts\Reset_Update_ShipRegion.sql");
            }
        }

        [Test]
        public void Should_update_field_to_null()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);
                }

                // alter property and then update -- Act
                toUpdate.ShipName = null;

                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTestDataContext())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.IsNull(original.ShipName);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                DBHelper.Execute(@"..\..\TestScripts\Reset_Update_ShipName.sql");
            }
        }        
    }
}