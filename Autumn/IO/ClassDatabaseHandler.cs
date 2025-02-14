namespace Autumn.IO;

internal static class ClassDatabaseHandler
{
    public class DatabaseEntry
    {
        public Dictionary<string, Arg> Args { get; set; }
        public string ClassName { get; set; }
        public string ClassNameFull { get; set; }
        public string Description { get; set; }
        public string DescriptionAdditional { get; set; }
        public string Name { get; set; }
        public bool RailRequired { get; set; }
        public Dictionary<string, Switch> Switches { get; set; }
    }

    public class Arg
    {
        public object Default { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
    }

    public class Switch
    {
        public string Description { get; set; }
        public string Type { get; set; }
    }

    private static SortedDictionary<string, DatabaseEntry> s_DatabaseEntries = null;

    public static SortedDictionary<string, DatabaseEntry> DatabaseEntries
    {
        get
        {
            if (s_DatabaseEntries is null)
            {
                s_DatabaseEntries = new();
                string path = Path.Join(Path.Join("Resources", "RedPepper-ClassDataBase"), "Data");

                foreach (string entryPath in Directory.EnumerateFiles(path))
                {
                    string className = Path.GetFileName(entryPath);
                    DatabaseEntry entry = YAMLWrapper.Deserialize<DatabaseEntry>(entryPath);
                    s_DatabaseEntries[entry.ClassName] = entry;
                }
            }

            return s_DatabaseEntries;
        }
    }
}