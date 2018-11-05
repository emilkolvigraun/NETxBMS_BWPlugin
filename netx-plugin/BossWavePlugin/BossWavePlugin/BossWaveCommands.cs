using BWBinding;
using BWBinding.Common;
using BWBinding.Interfaces;
using BWBinding.Utils;
using nxaXIO.PlugKit.Logging;
using nxaXIO.PlugKit.Nodes;
using nxaXIO.PlugKit.Nodes.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BossWavePlugin
{
    public class BossWaveCommands
    {
        private MessageHandler msgHandler;
        public List<string> entities;
        public BossWaveClient bwClient;

        public BossWaveCommands()
        {
            bwClient = new BossWaveClient("localhost", BWDefaults.DEFAULT_PORT_NUMBER);
        }

        public void Init()
        {
            entities = new List<string>();
            this.msgHandler = new MessageHandler();
            bwClient.Connect();
            PlugLog.memoryLog.Add("INFO, Connected to BossWave, " + DateTime.Now.ToString());
        }

        public void SetEntity(string path)
        {
            try
            {
                if (BossWavePlugin.Instance != null)
                {
                    foreach (var entity in BossWavePlugin.Instance.host.GetBranch(@"NETx\XIO\BossWave\Identity").BranchFacade.GetItems())
                    {
                        if (entity.GetValue().ToString().Length == 0)
                        {
                            if (entities.ToArray().Length == 0 || !entities.Any(item => entity.GetValue().ToString() == path))
                            {
                                entities.Add(path);
                                entity.SetValue(new UpdateRequest(path, ItemChangeReason.IoReceived));
                                bwClient.SetEntity(path, new ResponseHandler(path));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message);
            }
        }

        public void Subscribe(string ns)
        {
            try
            {
                if (BossWavePlugin.Instance != null)
                {
                    foreach (var sub in BossWavePlugin.Instance.host.GetBranch(@"NETx\XIO\BossWave\Subscriptions").BranchFacade.GetItems())
                    {
                        if (sub.GetValue().ToString().Length == 0)
                        {
                            if (BossWavePlugin.Instance.bwItems.ToArray().Length == 0 || !BossWavePlugin.Instance.bwItems.Any(item => sub.GetValue().ToString() == ns))
                            {
                                BossWavePlugin.Instance.bwItems.Add(sub.ItemId);
                                sub.SetValue(new UpdateRequest(ns, ItemChangeReason.IoReceived));

                                Request subscribeRequest = new RequestUtils(ns, RequestType.SUBSCRIBE)
                                    .SetAutoChain(true)
                                    .BuildSubcriber();

                                bwClient.Subscribe(subscribeRequest, new ResponseHandler(ns), msgHandler);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message + " " + e);
            }
        }

        public void Publish(string ns, string primaryAccessChain, string payload)
        {
            try { 
                if (BossWavePlugin.Instance != null)
                {
                    RequestUtils publishRequestUtils = new RequestUtils(ns, RequestType.PUBLISH)
                    .SetPrimaryAccessChain(primaryAccessChain);
                    byte[] message = Encoding.UTF8.GetBytes(payload);
                    byte[] text = { 64, 0, 0, 0 };

                    PayloadObject payloadObject = new PayloadObject(new PayloadType(text), message);
                    publishRequestUtils.AddPayloadObject(payloadObject);
                    Request publishRequest = publishRequestUtils.BuildPublisher();

                    bwClient.Publish(publishRequest, new TempResponseHandler());
                }
            }
            catch (Exception e)
            {
                BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message + " " + e);
            }

        }

        public void Stop()
        {
            PlugLog.memoryLog.Add("WARNING, Session ended at, " + DateTime.Now.ToString());
            bwClient.Dispose();
        }
    }

    class TempResponseHandler : IResponseHandler
    {
        public Response result { get; set; }

        public bool received { get; set; }

        public void ResponseReceived(Response result)
        {
            // For convenience
        }
    }
}
