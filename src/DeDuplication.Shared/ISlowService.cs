using Akka.Util;
using System;
using System.Threading.Tasks;

namespace DeDuplication.Shared
{
    /// <summary>
    /// Used to simulate an _extremely_ slow service.
    /// </summary>
    public interface ISlowService
    {
        Task<Response> Process(Request req);
    }

    public sealed class SlowService : ISlowService
    {
        public async Task<Response> Process(Request req)
        {
            // Task.Delay is in milliseconds
            await Task.Delay(ThreadLocalRandom.Current.Next(100, 4000));
            return new Response(req.RequestId, ThreadLocalRandom.Current.Next());
        }
    }
}
