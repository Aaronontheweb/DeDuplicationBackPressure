namespace DeDuplication.Shared
{
    public sealed class Request
    {
        public long RequestId { get; }

        public Request(long requestId)
        {
            RequestId = requestId;
        }
    }
}
