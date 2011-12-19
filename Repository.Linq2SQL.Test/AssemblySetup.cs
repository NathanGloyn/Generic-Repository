using System.Configuration;
using NUnit.Framework;
using UnitTest.Database;

namespace Repository.Linq2SQL.Test
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [SetUp]
        public void Initalize()
        {
            var DbHelper = new DatabaseSupport(ConfigurationManager.ConnectionStrings["testDB"].ConnectionString);

            DbHelper.RunScript(@"..\..\TestScripts\Setup\01_Create_DataBase.sql");
            DbHelper.RunScript(@"..\..\TestScripts\Setup\02_Create_Table.sql");
            DbHelper.RunScript(@"..\..\TestScripts\Setup\03_Insert_Order.sql");
        }   
    }
}