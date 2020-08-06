using Akka.Actor;
using Akka.Persistence.Extras;

namespace DeDuplication.Shared
{
    public sealed class DeliveryPayloadData
    {
        public DeliveryPayloadData(IActorRef sender, ConfirmableMessageEnvelope confirmableMessageEnvelope)
        {
            Sender = sender;
            ConfirmableMessage = confirmableMessageEnvelope;
        }

        public IActorRef Sender { get; }

        public ConfirmableMessageEnvelope ConfirmableMessage { get; }

        public object MessageToBeProcessed => ConfirmableMessage.Message;

        public DeliveryPayloadData WithNewPayload(object payload)
        {
            return new DeliveryPayloadData(Sender, new ConfirmableMessageEnvelope(ConfirmableMessage.ConfirmationId, ConfirmableMessage.SenderId, payload));
        }
    }
}
