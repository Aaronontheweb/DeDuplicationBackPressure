using Akka.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeDuplication.Shared
{
    /// <summary>
    /// Used to simulate an _extremely_ slow service.
    /// </summary>
    public interface ISlowService
    {
        Task<Response> Process(Request req);

        int DuplicateCalls { get; }
    }

    public sealed class SlowService : ISlowService
    {
        private int _duplicateCalls = 0;
        public int DuplicateCalls
        {
            get
            {
                lock (this)
                {
                    return _duplicateCalls;
                }
            }
            set
            {
                lock (this)
                {
                    _duplicateCalls = value;
                }
            }
        }
        private readonly HashSet<long> _ids = new HashSet<long>();

        public async Task<Response> Process(Request req)
        {
            // Task.Delay is in milliseconds
            await Task.Delay(ThreadLocalRandom.Current.Next(100, 40000));
            if (_ids.Contains(req.RequestId))
            {
                DuplicateCalls++;
            }
            _ids.Add(req.RequestId);
            return new Response(req.RequestId, ThreadLocalRandom.Current.Next());
        }
    }
}
