using System;
using NUnit.Framework;
using Repository.Test.Common;

namespace Repository.EntityFramework.Test
{
    [TestFixture]
    public class When_updating_data
    {
        [Test]
        public void Should_throw_exception_if_null_passed_in()
        {
            using (var context = new RepositoryTest())
            {
                var repo = new Repository<Order>(context);

                Assert.Throws<ArgumentNullException>(() => repo.Update(null));
            }
        }

        [Test]
        public void Should_update_entity_from_disconnected_context()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);
                }

                // alter property and then update -- Act
                toUpdate.ShipName = "abc";

                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.AreEqual("abc", original.ShipName);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                TestScriptHelper.ResetShipName();
            }
        }

        [Test]
        public void Should_update_entity_using_single_context()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);


                    // alter property and then update -- Act
                    toUpdate.ShipName = "abc";

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.AreEqual("abc", original.ShipName);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                TestScriptHelper.ResetShipName();
            }
        }  

        [Test]
        public void Should_update_null_property_to_given_value()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);
                }

                toUpdate.ShipRegion = "West";

                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.AreEqual("West", original.ShipRegion);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                TestScriptHelper.ResetShipRegion();
            }
        }

        [Test]
        public void Should_update_field_to_null()
        {
            Order toUpdate = null;

            try
            {
                // Get original  -- Arrange
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    toUpdate = repo.GetById(10337);
                }

                // alter property and then update -- Act
                toUpdate.ShipName = null;

                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    repo.Update(toUpdate);
                }

                // Requery db to Assert that data updated
                using (var context = new RepositoryTest())
                {
                    var repo = new Repository<Order>(context);

                    var original = repo.GetById(10337);

                    Assert.IsNull(original.ShipName);
                }
            }
            finally
            {
                // Clean up the data so that its in the correct state
                TestScriptHelper.ResetShipName();
            }
        }        
    }
}