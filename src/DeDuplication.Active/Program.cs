using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Configuration;
using Akka.Persistence.Extras;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace DeDuplication.Active
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf"));
            var actorSystem = ActorSystem.Create("DeDupSys", config);

            var pbm = PetabridgeCmd.Get(actorSystem);
            pbm.Start(); // begin listening for PBM management commands
            
            await actorSystem.WhenTerminated;
        }
    }

    public sealed class ActiveDeDuplicatingActor : DeDuplicatingReceiveActor
    {
        public override string PersistenceId => throw new System.NotImplementedException();

        protected override object CreateConfirmationReplyMessage(long confirmationId, string senderId, object originalMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
