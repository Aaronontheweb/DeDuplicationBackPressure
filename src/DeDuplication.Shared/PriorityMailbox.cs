using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Persistence.Extras;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeDuplication.Shared
{
    public class PriorityMailbox : UnboundedStablePriorityMailbox
    {
        public PriorityMailbox(Settings settings, Config config) : base(settings, config)
        {
        }

        protected override int PriorityGenerator(object message)
        {
            switch (message)
            {
                case ConfirmableMessageEnvelope cme when cme.Message is Response:
                    return 0;
                default:
                    return 100;
            }
        }
    }
}
