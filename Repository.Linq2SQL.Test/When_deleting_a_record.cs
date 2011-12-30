using System;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [TestFixture]
    public class When_deleting_a_record
    {
        

        [Test]
        public void Should_throw_ArgumentNullException_if_null_passed_in()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                Assert.Throws<ArgumentNullException>(() => repo.Delete(null));
            }
        }

        [Test]
        public void Should_delete_record_when_given_a_valid_entity_from_a_seperate_data_context()
        {
            Order original;

            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                original = repo.GetById(10287);
            }

            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);

                repo.Delete(original);
            }

            Assert.IsTrue(CheckRecordDeleted());

            TestScriptHelper.InsertDeletedRecord();
        }

        [Test]
        public void Should_delete_the_record_when_given_a_valid_entity_on_the_same_data_context()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                var original = repo.GetById(10287);

                repo.Delete(original);
            }

            Assert.IsTrue(CheckRecordDeleted());

            TestScriptHelper.InsertDeletedRecord();
        }

        private bool CheckRecordDeleted()
        {
            using (var context = new RepositoryTestDataContext())
            {
                var repo = new Repository<Order>(context);
                try
                {
                    repo.GetById(10287);
                }
                catch (InvalidOperationException ioe)
                {
                    return true;
                }

                return false;
            }            
        }
    }
}
