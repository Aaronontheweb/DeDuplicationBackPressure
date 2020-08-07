using System;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Extras;
using Akka.Streams;
using Akka.Streams.Dsl;
using DeDuplication.Shared;

namespace DeDuplication.Passive
{
    public sealed class PassiveDeDuplicatingActor : DeDuplicatingReceiveActor, IWithTimers
    {
        public override string PersistenceId { get; }

        private int _messagesConfirmed;
        private int _messagesDeduped;
        private int _parallelismSetting = 8;
        private IActorRef _msgQueue;

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly ISlowService _slowService;

        public PassiveDeDuplicatingActor(string persistenceId, ISlowService slowService)
        {
            PersistenceId = persistenceId;
            _slowService = slowService;

            Command<Request>(r =>
            {
                _msgQueue.Tell(CreateStreamsPayload(r));
            });

            Command<Response>(d =>
            {
                _messagesConfirmed++;
                ConfirmAndReply(d.RequestId);
            });

            Command<FlushStats>(f =>
            {
                _log.Info("Messages confirmed [{0}] / Messages deduplicated [{1}] / Duplicate Service Calls [{2}]", _messagesConfirmed, _messagesDeduped, _slowService.DuplicateCalls);
            });
        }

        private DeliveryPayloadData CreateStreamsPayload(object message)
        {
            if (!IsCurrentMessageConfirmable)
                throw new InvalidOperationException($"Can't pass message {message} through Akka.Streams - wasn't sent via IConfirmableMessage or ConfirmableMessageEnvelope");
            return new DeliveryPayloadData(Sender, new ConfirmableMessageEnvelope(CurrentConfirmationId.Value, CurrentSenderId, message));
        }

        public ITimerScheduler Timers { get; set; }

        private long CreateResponse(long requestId)
        {
            return requestId % 3;
        }

        protected override void HandleDuplicate(long confirmationId, string senderId, object duplicateMessage)
        {
            _messagesDeduped++;
            base.HandleDuplicate(confirmationId, senderId, duplicateMessage);
        }

        protected override object CreateConfirmationReplyMessage(long confirmationId, string senderId, object originalMessage)
        {
            return new Response(confirmationId, CreateResponse(confirmationId));
        }

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer("MetricsFlush", FlushStats.Instance, TimeSpan.FromSeconds(10));

            var self = Self;

            _msgQueue = Source.ActorRef<DeliveryPayloadData>(1000, OverflowStrategy.DropHead)
                .Where(x =>
                {
                    switch (x.MessageToBeProcessed)
                    {
                        case Request ci:
                            return true;
                        default:
                            return false;
                    }
                })
                .SelectAsyncUnordered(_parallelismSetting, async x => // maximum of 8 concurrent requests at a time
                {
                    // should add error handling inside of here
                    switch (x.MessageToBeProcessed)
                    {
                        case Request r:
                            return x.WithNewPayload(await _slowService.Process(r));
                        default:
                            return x; // return the original message if it's not something we can handle (need to log this)
                    }
                })
                .To(Sink.ForEach<DeliveryPayloadData>(c =>
                {
                    // bring the original Sender back
                    self.Tell(c.ConfirmableMessage, c.Sender);
                }))
                .Run(Context.Materializer());
        }
    }
}
