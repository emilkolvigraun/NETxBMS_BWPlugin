using BWBinding.Common;
using BWBinding.Interfaces;
using BWBinding.Utils;
using Newtonsoft.Json.Linq;
using nxaXIO.PlugKit.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BossWavePlugin.Host
{
    class MessageHandler : IMessageHandler
    {
        public Message message { get; set; }

        public bool received { get; set; }

        public void ResultReceived(Message message)
        {
            Dictionary<string, string> log_msg = new Dictionary<string, string>();

            log_msg.Add("message", message.payloadObjects.ToString());
            log_msg.Add("timestamp", DateTime.UtcNow.ToString());

            LogHub.AddToLog(log_msg.ToString());


            if (BossWavePlugin.Instance != null)
            {
                try
                {
                    JObject info = BossWavePlugin.Instance.config;
                    RequestUtils publishRequestUtils = new RequestUtils(info["namespace-to-pub"].ToString(), RequestType.PUBLISH).SetPrimaryAccessChain(info["primary-access-chain-to-pub"].ToString());

                    byte[] pubmsg = Encoding.UTF8.GetBytes(message.payloadObjects.ToString());

                    byte[] text = { 64, 0, 0, 0 };

                    PayloadObject payload = new PayloadObject(new PayloadType(text), pubmsg);
                    publishRequestUtils.AddPayloadObject(payload);
                    Request publishRequest = publishRequestUtils.BuildPublisher();

                    BossWavePlugin.Instance.bwClient.Publish(publishRequest, new ResponseHandler("publish"));
                }
                catch (Exception e)
                {
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message);
                }
            }
        }
    }
}
