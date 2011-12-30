using System.Configuration;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [SetUp]
        public void Initalize()
        {
            TestScriptHelper.SetUpDatabase();
        }   

    }
}