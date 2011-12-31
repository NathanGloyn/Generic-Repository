using System;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_constructing_repository
    {
        [Test]
        public void Should_throw_ArgumentNullException_if_no_context_supplied()
        {
            Assert.Throws<ArgumentNullException>(() => new Repository<Order>(null));
        }
    }
}