

public class WorldMetadata
{
    public WorldMeta world { get; set; }
    public GenerationMeta generation { get; set; }
    public TimestampMeta timestamps { get; set; }

    public class WorldMeta
    {
        public string id { get; set; }
        public string name { get; set; }
        public string layer { get; set; }
        public string version { get; set; }
    }

    public class GenerationMeta
    {
        public int seed { get; set; }
        public int type { get; set; }
    }

    public class TimestampMeta
    {
        public long created { get; set; }
        public long lastAccessed { get; set; }
    }
}