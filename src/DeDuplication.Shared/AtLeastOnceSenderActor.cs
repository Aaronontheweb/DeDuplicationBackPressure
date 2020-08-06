using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using Akka.Persistence.Extras;
using Akka.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeDuplication.Shared
{

    public sealed class AtLeastOnceSenderActor : AtLeastOnceDeliveryReceiveActor, IWithTimers
    {
        public override string PersistenceId { get; }
        public ITimerScheduler Timers { get; set; }

        private readonly IActorRef _receivers;

        private int _messagesSent;
        private int _messagesConfirmed;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        public AtLeastOnceSenderActor(string persistenceId, IActorRef receivers)
        {
            PersistenceId = persistenceId;
            _receivers = receivers;

            Recover<SnapshotOffer>(s =>
            {
                if(s.Snapshot is AtLeastOnceDeliverySnapshot ald)
                {
                    SetDeliverySnapshot(ald);
                }
            });

            // Create a batch of messages we're going to need to "At least once" deliver
            Command<int>(i =>
            {
                foreach(var n in Enumerable.Range(0, ThreadLocalRandom.Current.Next(0, i)))
                {
                    _messagesSent++;
                    Deliver(_receivers.Path, id => new ConfirmableMessageEnvelope(id, PersistenceId, new Request(id)));
                }
            });

            Command<Response>(r =>
            {
                ConfirmDelivery(r.RequestId);
                _messagesConfirmed++;
            });

            Command<FlushStats>(_ =>
            {
                _log.Info("Messages sent [{0}] / Messages confirmed [{1}] / Outstanding Messages [{2}]", _messagesSent, _messagesConfirmed, _messagesSent - _messagesConfirmed);
            });
        }

        public override void AroundPreStart()
        {
            Timers.StartPeriodicTimer("MetricsFlush", FlushStats.Instance, TimeSpan.FromSeconds(1));
            Timers.StartPeriodicTimer("MessagesPush", 25, TimeSpan.FromSeconds(5));
        }
    }
}
