namespace DeDuplication.Shared
{
    public sealed class Response
    {
        public long RequestId { get; }

        public Response(long requestId, long data)
        {
            RequestId = requestId;
            Data = data;
        }

        public long Data { get; }
    }
}
