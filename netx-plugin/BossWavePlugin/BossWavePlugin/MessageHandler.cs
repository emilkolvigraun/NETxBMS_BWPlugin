using BWBinding.Common;
using BWBinding.Interfaces;
using Newtonsoft.Json.Linq;
using nxaXIO.PlugKit.Nodes;
using nxaXIO.PlugKit.Nodes.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BossWavePlugin
{
    class MessageHandler : IMessageHandler
    {
        public Message message { get; set; }

        public bool received { get; set; }

        public void ResultReceived(Message message)
        {
            var instance = BossWavePlugin.Instance;
            if (instance != null)
            {
                try
                {
                    byte[] byteLoad = message.payloadObjects[0].load;

                    JObject payloadJsonObject = JObject.Parse(Encoding.UTF8.GetString(byteLoad));

                    Dictionary<string, string> dictObj = payloadJsonObject.ToObject<Dictionary<string, string>>();

                    if (!dictObj.ContainsKey("level"))
                    {
                        instance.host.GetItem(payloadJsonObject["itemid"].ToString()).ItemFacade.SetValue(new UpdateRequest(payloadJsonObject["value"].ToString(), ItemChangeReason.IoReceived));
                        
                        PlugLog.memoryLog.Add("RESULT-RECEIVED, " + payloadJsonObject["value"].ToString() + ", " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ", " + payloadJsonObject["itemid"].ToString());
                    }

                    
                }
                catch(Exception e)
                {
                    instance.host.WriteLog(nxaXIO.PlugKit.Logging.LogLevel.Error, e.Message + " " + e);
                }
            }
        }
    }
}
