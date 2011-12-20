using System.Linq;
using NUnit.Framework;
using Satisfyr;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_retrieving_all_records
    {
        [Test]
        public void Should_return_a_enumerable_with_records()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                CollectionAssert.IsNotEmpty(repo.GetAll());
            }
        }

        [Test]
        public void Should_correct_number_of_records()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                Assert.AreEqual(100, repo.GetAll().Count());
            }
        }

        [Test]
        public void Should_have_correct_first_and_last_item()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                var items = repo.GetAll();

                items.First().Satisfies(o => o.OrderID == 10248
                      && o.ShipVia == 3
                      && o.ShipName == "Vins et alcools Chevalier");

                items.Last().Satisfies(o => o.OrderID == 10347
                      && o.ShipVia == 3
                      && o.ShipName == "Familia Arquibaldo");
            }            
        }

    }
}