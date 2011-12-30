using System.Configuration;
using NUnit.Framework;
using Repository.Test.Common;

namespace Repository.EntityFramework.Test
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