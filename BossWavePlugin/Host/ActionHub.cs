using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BossWavePlugin.Host
{
    [HubName("actionHub")]
    public class ActionHub : Hub
    {

        public void SetItemValue(string itemID,string itemValue)
        {
            if (BossWavePlugin.Instance != null)
            {
                BossWavePlugin.Instance.SetItemValue(itemID,itemValue);
            }         
        }

        public void Inform(string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ActionHub>();
            context.Clients.All.Inform(message);
        }
    }
}
