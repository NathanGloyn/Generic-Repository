using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace Repository.Linq2SQL.Test
{
    public class TestScriptHelper
    {
        private const string scriptFolder = @"..\..\..\Repository.Test.Common\TestScripts\";

        public static void SetUpDatabase()
        {
            DBHelper.Execute(Path.Combine(scriptFolder,@"Setup\01_Create_DataBase.sql"));
            DBHelper.Execute(Path.Combine(scriptFolder,@"Setup\02_Create_Table.sql"));
            DBHelper.Execute(Path.Combine(scriptFolder,@"Setup\03_Insert_Order.sql"));
        }   

        public static void ResetData()
        {
            DBHelper.Execute(Path.Combine(scriptFolder,@"Setup\04_Reset_Data.sql"));
        }

        public static void InsertDeletedRecord()
        {
            DBHelper.Execute(Path.Combine(scriptFolder, "Insert_deleted_order.sql"));
        }

        public static void ResetShipName()
        {
            DBHelper.Execute(Path.Combine(scriptFolder, "Reset_Update_ShipName.sql"));
        }

        public static void ResetShipRegion()
        {
            DBHelper.Execute(Path.Combine(scriptFolder, "Reset_Update_ShipRegion.sql"));
        }
    }
}