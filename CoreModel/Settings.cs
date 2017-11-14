namespace CoreModel
{
    public enum ComparisonType
    {
        ByName,
        By2KBInitialBytes
    }
    public class Settings
    {
        public ComparisonType ComparisonType { get; set; }
        public int Interval { get; set; }
    }
}
