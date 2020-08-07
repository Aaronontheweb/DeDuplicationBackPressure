using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using DeDuplication.Shared;

namespace DeDuplication.Active
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf"));
            var actorSystem = ActorSystem.Create("ActiveSys", config);

            var receivers = actorSystem.ActorOf(Props.Create(() => new ActiveDeDuplicatingActor("dedup", new SlowService())).WithMailbox("priority-mailbox"), "deduplicating");
            var sender = actorSystem.ActorOf(Props.Create(() => new AtLeastOnceSenderActor("sender", receivers)), "sender");
            
            await actorSystem.WhenTerminated;
        }
    }
}
