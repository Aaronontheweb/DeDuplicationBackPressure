using Akka.Actor;
using Akka.Persistence;
using Akka.Persistence.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeDuplication.Shared
{
    public sealed class AtLeastOnceSenderActor : AtLeastOnceDeliveryReceiveActor
    {
        public override string PersistenceId { get; }

        private readonly IActorRef _receivers;

        private int _messagesSent;
        private int _messagesConfirmed;

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
                foreach(var n in Enumerable.Range(0, i))
                {
                    Deliver(_receivers.Path, id => new ConfirmableMessageEnvelope(id, PersistenceId, new Request(id)));
                }
            });
        }
    }
}
