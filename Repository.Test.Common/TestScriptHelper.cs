using System.IO;

namespace Repository.Test.Common
{
    public class TestScriptHelper
    {
        private const string ScriptFolder = @"..\..\..\Repository.Test.Common\TestScripts\";

        public static void SetUpDatabase()
        {
            DBHelper.Execute(Path.Combine(ScriptFolder,@"Setup\01_Create_DataBase.sql"));
            DBHelper.Execute(Path.Combine(ScriptFolder,@"Setup\02_Create_Table.sql"));
            DBHelper.Execute(Path.Combine(ScriptFolder,@"Setup\03_Insert_Order.sql"));
        }   

        public static void ResetData()
        {
            DBHelper.Execute(Path.Combine(ScriptFolder,@"Setup\04_Reset_Data.sql"));
        }

        public static void InsertDeletedRecord()
        {
            DBHelper.Execute(Path.Combine(ScriptFolder, "Insert_deleted_order.sql"));
        }

        public static void ResetShipName()
        {
            DBHelper.Execute(Path.Combine(ScriptFolder, "Reset_Update_ShipName.sql"));
        }

        public static void ResetShipRegion()
        {
            DBHelper.Execute(Path.Combine(ScriptFolder, "Reset_Update_ShipRegion.sql"));
        }
    }
}