using DbUp;
using System;
using System.Reflection;

namespace TemperatureDatabases
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name}.exe <connectionString>");
                return -1;
            }

            try
            {
                var connectionString = args[0];

                EnsureDatabase.For.MySqlDatabase(connectionString);

                var databaseUpgradeResult = DeployChanges.To
                    .MySqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build()
                    .PerformUpgrade();

                return databaseUpgradeResult.Successful ? 0 : -1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Exception caught while running: {e}");
                return -1;
            }
        }
    }
}
