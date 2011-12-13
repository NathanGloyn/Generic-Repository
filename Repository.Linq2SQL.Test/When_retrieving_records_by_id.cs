using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_retrieving_records_by_id
    {
        [Test]
        public void Should_throw_argument_exception_if_id_less_than_zero_provided()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                Assert.Throws<ArgumentException>(() => repo.GetById(-1));
            }
        }

        [Test]
        public void Should_return_object_for_id_provided()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                var actual = repo.GetById(10248);

                Assert.IsNotNull(actual);
            }
        }
    }
}
