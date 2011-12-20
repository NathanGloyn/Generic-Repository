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
            DBHelper.Execute(@"..\..\TestScripts\Setup\01_Create_DataBase.sql");
            DBHelper.Execute(@"..\..\TestScripts\Setup\02_Create_Table.sql");
            DBHelper.Execute(@"..\..\TestScripts\Setup\03_Insert_Order.sql");
        }   

    }
}