using Microsoft.AspNet.SignalR;

namespace BossWave
{
    public class ActionHub : Hub
    {
        public void SetEntitiy(string path)
        {

        }

        public void Subscribe(string ns)
        {

        }

        private void Publish(string obj)
        {

        }

        public void SetPublish(string obj, int seconds)
        {

        }

        public void Announce(string message)
        {
            Clients.All.Announce(message);
        }
    }
}