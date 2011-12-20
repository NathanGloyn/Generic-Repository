using System;
using System.Linq;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_finding_records
    {
        [Test]
        public void Should_throw_exception_if_null_criteria_provided()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                Assert.Throws<ArgumentNullException>(() => repo.Find(null));
            }            
        }
        
        [Test]
        public void Should_return_correct_number_of_records_based_on_criteria()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                Assert.AreEqual(3, repo.Find(o => o.CustomerID == "VINET").Count());
            }     
        }

        [Test]
        public void Should_return_empty_enumerable_where_no_records_match_criteria()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                CollectionAssert.IsEmpty(repo.Find(o => o.CustomerID == "NoMatch"));
            }     
        }

    }
}