namespace PinnyNotes.Core.Configurations;

public class DatabaseConfiguration
{
    public const string DatabaseFileName = "pinny_notes.sqlite";

    public readonly string DataPath;
    public readonly string ConnectionString;

    public string DatabaseFilePath => Path.Combine(DataPath, DatabaseFileName);

    public DatabaseConfiguration()
    {
        // Use exe dir for database if in Debug mode or is portable.
        if (System.Diagnostics.Debugger.IsAttached || File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.txt")))
            DataPath = AppContext.BaseDirectory;
        else
            DataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Pinny Notes"
            );

        if (!Path.Exists(DataPath))
            Directory.CreateDirectory(DataPath);

        ConnectionString = $"Data Source={Path.Combine(DataPath, DatabaseFileName)}";
    }
}
