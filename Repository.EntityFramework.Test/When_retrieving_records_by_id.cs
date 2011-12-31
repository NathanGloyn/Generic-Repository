using System;
using System.Data;
using NUnit.Framework;
using Satisfyr;

namespace Repository.EntityFramework.Test
{
    [TestFixture]
    public class When_retrieving_records_by_id
    {
        [Test]
        public void Should_throw_ArgumentNullException_if_null_id_provided()
        {
            using (var context = new RepositoryTest())
            {
                using (var repo = new Repository<Order>(context))
                {
                    Assert.Throws<ArgumentNullException>(() => repo.GetById(null));
                }
            }
        }

        [Test]
        public void Should_return_an_object_for_id_provided()
        {
            using (var context = new RepositoryTest())
            {
                var repo = new Repository<Order>(context);

                Assert.IsNotNull(repo.GetById(10248));
            }
        }
        
        [Test]
        public void Should_return_correct_object_for_id()
        {
            using (var context = new RepositoryTest())
            {
                var repo = new Repository<Order>(context);

                var actual = repo.GetById(10248);

                actual.Satisfies(o => o.OrderID == 10248
                                      && o.ShipVia == 3
                                      && o.ShipName == "Vins et alcools Chevalier");
            }            
        }

        [Test]
        public void Should_throw_InvalidOperationException_if_invalid_id_provided()
        {
            using (var context = new RepositoryTest())
            {
                var repo = new Repository<Order>(context);

                Assert.Throws<ObjectNotFoundException>(() => repo.GetById(1));
            }
        }


    }
}
