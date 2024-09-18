// See https://aka.ms/new-console-template for more information

namespace Example.DatabaseMigration;

internal static class Program
{
    public static int Main(string[] args)
    {
        // If you are migrating to SQL Server Express use connection string "Server=(LocalDb)\\MSSQLLocalDB;..."
        // If you are migrating to SQL Server use connection string "Server=localhost;..."
        var connectionString =
            args.FirstOrDefault()
            ?? "Server=.;Database=outbox;Trusted_Connection=True;Encrypt=No;";

        Console.WriteLine($"Performing upgrade on {connectionString}");
        var result = DbUpgrader.DatabaseUpgrade(connectionString);

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
    }
}
