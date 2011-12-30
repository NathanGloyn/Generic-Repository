using System.Configuration;
using UnitTest.Database;

namespace Repository.Test.Common
{
    public class DBHelper
    {
        private static DatabaseSupport dbSupport;

        private static DatabaseSupport DbSupport
        {
            get
            {
                if (dbSupport == null)
                {
                    dbSupport = new DatabaseSupport(ConfigurationManager.ConnectionStrings["testDB"].ConnectionString);
                }
                return dbSupport;
            }
        }

        public static void Execute(string pathToScript)
        {
            DbSupport.RunScript(pathToScript);
        }
    }
}