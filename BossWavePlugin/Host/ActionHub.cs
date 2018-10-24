using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BossWavePlugin.Host
{
    [HubName("actionHub")]
    public class ActionHub : Hub
    {
        public void InformOther(string msg) { 
            Clients.Others.inform(msg);
        }

        public void ToggleEntity()
        {
            if (BossWavePlugin.Instance != null)
            {
                if (BossWavePlugin.Instance.config != null)
                {
                    SetEntity(BossWavePlugin.Instance.config["entity-path"].ToString());
                }
            }
        }

        public void SetEntity(string path)
        {
            if (BossWavePlugin.Instance != null)
            {
                if(BossWavePlugin.Instance.SetEntity(path))
                {
                    Clients.Client(Context.ConnectionId).entityHasBeenSet(path);
                } 
            }
        }

        public void GetItemInfo(string type, string title)
        {
            if (type.Equals("sub"))
            {
                if (BossWavePlugin.Instance != null)
                {
                    Clients.Client(Context.ConnectionId).getSubItem(BossWavePlugin.Instance.bwSubItems[title]);
                }
            } else if (type.Equals("pub"))
            {
                if (BossWavePlugin.Instance != null)
                {
                    Clients.Client(Context.ConnectionId).getPubItem(BossWavePlugin.Instance.bwPubItems[title]);
                }
            }
        }

        public void CreateSubscription(string information)
        {
            if (BossWavePlugin.Instance != null)
            {
                if (BossWavePlugin.Instance.CreateSubItem(information))
                {
                    Clients.All.updateSub(BossWavePlugin.Instance.bwSubItems.Keys);
                }              
            }
        }

        public void CreatePublishing(string info)
        {
            if (BossWavePlugin.Instance != null)
            {
                if (BossWavePlugin.Instance.CreatePubItem(info))
                {
                    Clients.Client(Context.ConnectionId).updatePub(BossWavePlugin.Instance.bwPubItems.Keys);
                }
            }
        }

        public void LoadSubscriptions()
        {
            if (BossWavePlugin.Instance != null)
            {
                Clients.Client(Context.ConnectionId).updateSub(BossWavePlugin.Instance.bwSubItems.Keys);
            }
        }
        public void LoadPublishes()
        {
            if (BossWavePlugin.Instance != null)
            {
                Clients.Client(Context.ConnectionId).updatePub(BossWavePlugin.Instance.bwPubItems.Keys);
            }
        }
    }
}
