using System;
using System.Collections.Generic;
using System.Timers;
using Newtonsoft.Json.Linq;
using nxaXIO.PlugKit.Nodes;
using nxaXIO.PlugKit.Plugin;
using nxaXIO.PlugKit.Plugin.Results;
using BWBinding;
using BWBinding.Utils;
using System.Text;
using BWBinding.Common;
using System.Threading;
using nxaXIO.PlugKit.Nodes.Requests;

namespace TestPlugin.DemoPl2
{
    [PluginInfo("KNX", "Mockup Items for KNX", "1.0.0")]
    public class Dpl2 : PluginBase
    {
        List<string> itemPaths;
        Thread pubThread;
        public IPluginHost host;

        public override bool Init(uint data1, uint data2)
        {
            host = mHost_;
            itemPaths = new List<string>();

            itemPaths.Add(@"NETx\XIO\KNX\001");
            itemPaths.Add(@"NETx\XIO\KNX\011");
            itemPaths.Add(@"NETx\XIO\KNX\111");
            itemPaths.Add(@"NETx\XIO\KNX\Gateway1\100");
            itemPaths.Add(@"NETx\XIO\KNX\Gateway1\010");
            itemPaths.Add(@"NETx\XIO\KNX\Gateway2\110");
            
            return true;
        }

        public override bool RegisterItems()
        {
            ItemCreateOptions options = new ItemCreateOptions { Description = "Fictional device", Datatype = typeof(int), AccessRights = ItemAccess.ReadWrite };

            // creating mockup KNX item tree
            foreach (string path in itemPaths)
            {
                mHost_.CreateItem(path, options);
            }

            PlugLog.EnableLogging(120000);
            return true;
        }

        public override bool ShutDown()
        {
            pubThread.Abort();
            return true;
        }

        public override bool Start()
        {
            pubThread = new Thread(() => new Publisher().Run(itemPaths, host));
            pubThread.Start();
            return true;
        }

        public override void StateChange(BmsServerState oldState, BmsServerState newState){}

        public override bool Stop()
        {
            PlugLog.LogWriter.Stop();
            pubThread.Abort();
            return true;
        }
    }

    public class Publisher
    {
        static List<string> items;
        public static BossWaveClient bwClient;
        static int counter = 1;
        static System.Timers.Timer aTimer;
        static int nextAddOn = 50;
        static IPluginHost host;
        static object locker = new object();
        static Random rand = new Random();

        public void Run(List<string> items, IPluginHost host)
        {
            Publisher.items = items;
            Publisher.host = host;
            bwClient = new BossWaveClient("localhost", BWDefaults.DEFAULT_PORT_NUMBER);
            bwClient.Connect();
            bwClient.SetEntity("C:/Users/Emil S. Kolvig-Raun/stubbe.ent", BWDefaults.DEFAULT_RESPONSEHANDLER(1));

            aTimer = new System.Timers.Timer(30000); // A timer with a twenty second interval.

            aTimer.Elapsed += OnTimedEvent; // Hook up the Elapsed event for the timer. 
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static int GetNumber()
        {
            int seq;
            lock (locker)
            {
                seq = rand.Next(items.Count);
            }
            return seq;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (aTimer.Interval > 0)
            {
                string item = items[GetNumber()];
                string payload = new JObject(new JProperty("value", counter.ToString()), new JProperty("halfed-times", (counter / 50).ToString()), new JProperty("type", "knx"), new JProperty("itemid", item)).ToString();

                PlugLog.memoryLog.Add("RESULT-PUBLISHED, " + counter + ", " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ", " + item + ", " + aTimer.Interval);

                RequestUtils publishRequestUtils = new RequestUtils("netx_test.cfei.ba/sharptesting", RequestType.PUBLISH)
                            .SetPrimaryAccessChain("nw0ChNe30YJBq4Q1LzI2TefzcfypV6-vYYOc_cUaaRc=");
                byte[] message = Encoding.UTF8.GetBytes(payload);
                byte[] text = { 64, 0, 0, 0 };

                PayloadObject payloadObject = new PayloadObject(new PayloadType(text), message);
                publishRequestUtils.AddPayloadObject(payloadObject);
                Request publishRequest = publishRequestUtils.BuildPublisher();

                bwClient.Publish(publishRequest, BWDefaults.DEFAULT_RESPONSEHANDLER(0));


                if (counter > nextAddOn)
                {
                    aTimer.Interval = aTimer.Interval - (aTimer.Interval / 4);

                    nextAddOn += 50;
                }

                string item2 = items[GetNumber()];

                host.WriteLog(nxaXIO.PlugKit.Logging.LogLevel.Warning, "published: " + counter + ", timer: " + aTimer.Interval + ", item: " + item);
                host.WriteLog(nxaXIO.PlugKit.Logging.LogLevel.Warning, "modified: " + -counter + ", timer: " + aTimer.Interval + ", item: " + item2);
                
                PlugLog.memoryLog.Add("ITEM-CHANGED, " + -counter + ", " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ", " + item2 + ", " + aTimer.Interval);


                host.GetItem(item2).ItemFacade.SetValue(new UpdateRequest(-counter, ItemChangeReason.IoReceived));

                

                counter += 1;
            }
            else
            {
                PlugLog.LogWriter.Stop();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
        }
    }
}
