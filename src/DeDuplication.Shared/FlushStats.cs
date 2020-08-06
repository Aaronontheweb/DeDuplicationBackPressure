namespace DeDuplication.Shared
{
    public sealed class FlushStats
    {
        public static readonly FlushStats Instance = new FlushStats();

        private FlushStats() { }
    }
}
