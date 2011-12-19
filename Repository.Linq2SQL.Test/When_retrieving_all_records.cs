using System.Linq;
using NUnit.Framework;

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

    }
}