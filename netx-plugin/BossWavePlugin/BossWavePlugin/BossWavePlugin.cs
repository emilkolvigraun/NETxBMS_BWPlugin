using System.IO;
using System.Reflection;
using nxaXIO.PlugKit.Nodes;
using nxaXIO.PlugKit.Plugin;
using nxaXIO.PlugKit.Plugin.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Threading;

namespace BossWavePlugin
{
    [PluginInfo("BossWavePlugin", "BossWave Binding", "1.0.0")]
    public class BossWavePlugin : PluginBase
    {
        public IPluginHost host; // For public referencing the Plugin Host _mHost
        public static BossWavePlugin Instance { get; protected set; } // Using a singleton instance to communicate
        public BossWaveCommands bwCommands;
        public JObject config; // Configuration file
        public GenericObserver knxObserver; // Observer for KNX items
        public GenericObserver bwObserver; // Observer for BossWave items

        /** BossWave Item options (mostæy for the sake of descriptors) **/
        private ItemCreateOptions subscribeOptions;
        private ItemCreateOptions publishOptions;
        private ItemCreateOptions entityOptions;

        public List<string> bwItems;
        public Dictionary<string, int> knxItems;

        private static object kntItemsLock = new object();

        public BossWavePlugin()
        {
            Instance = this;
        }

        public override bool Init(uint data1, uint data2)
        {
            host = mHost_;

            bwCommands = new BossWaveCommands();

            bwItems = new List<string>();
            knxItems = new Dictionary<string, int>();

            /** Initializing the observers with the corresponding methods **/
            knxObserver = new GenericObserver(KNXItemValueChanged); 
            bwObserver = new GenericObserver(BWItemValueChanged);



            /** Loading the configuration file from the etx folder **/
            using (StreamReader r = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\etc\\bwconfig.json"))
            {
                config = JObject.Parse(r.ReadToEnd());
            }
            
            PlugLog.EnableLogging(120000); // Enable Logging

            /** Initializing all the options **/
            subscribeOptions = new ItemCreateOptions
            {
                Description = "Subscription",
                Datatype = typeof(string),
                AccessRights = ItemAccess.ReadWrite
            };

            publishOptions = new ItemCreateOptions
            {
                Description = "Publishing",
                Datatype = typeof(string),
                AccessRights = ItemAccess.ReadWrite
             };

            entityOptions = new ItemCreateOptions
            {
                Description = "Entity",
                Datatype = typeof(string),
                AccessRights = ItemAccess.ReadWrite
            };

            bwCommands.Init(); // Connect to BossWave

            return true;
        }

        public override bool RegisterItems()
        {
            CreateItems(@"NETx\XIO\BossWave\Identity\Ent", (int) config["entity-items"], entityOptions);
            CreateItems(@"NETx\XIO\BossWave\Subscriptions\Sub", (int) config["sub-items"], subscribeOptions);
            CreateItems(@"NETx\XIO\BossWave\Publishes\Pub", (int) config["pub-items"], publishOptions);
            
            RegisterItemObserver(@"NETx\XIO\BossWave\Identity", bwObserver);
            RegisterItemObserver(@"NETx\XIO\BossWave\Subscriptions", bwObserver);
            RegisterItemObserver(@"NETx\XIO\BossWave\Publishes", bwObserver);

            RegisterItemObserver(@"NETx\XIO\KNX", knxObserver);

            return true;
        }

        private void RegisterItemObserver(string branch, ItemObserver observer)
        {
            foreach (var item in host.GetBranch(branch).BranchFacade.GetItems()) // Getting top level items
            {
                item.RegisterObserver(observer);
            }
            foreach (var br in host.GetBranch(branch).BranchFacade.GetBranches()) // Getting all sub branches and their items
            {
                foreach (var item in br.GetItems())
                {
                    item.RegisterObserver(observer);
                }
            }
        }

        private void CreateItems(string branch, int amount, ItemCreateOptions options)
        {
            for (int i = 0; i < amount; i++)
            {
                host.CreateItem(branch + i, options);
            }
        }

        public override bool ShutDown()
        {
            bwCommands.Stop();
            LogWriter.Stop();
            return true;
        }

        public override bool Start()
        {            
            bwCommands.SetEntity(config["entity"].ToString());
            
            Dictionary<string, string> dictObj = config["subscriptions"].ToObject<Dictionary<string, string>>();

            foreach (var sub in dictObj.Keys)
            {
                Thread.Sleep(10);
                bwCommands.Subscribe(dictObj[sub]);
                Thread.Sleep(10);
            }
            
            return true;
        }

        public override void StateChange(BmsServerState oldState, BmsServerState newState){        }

        public override bool Stop()
        {
            return true;
        }

        private void BWItemValueChanged(object tag, IItemFacade facade)
        {
            string value = facade.GetValue().ToString();
            string itemid = facade.ItemId.ToString();

            if (!bwItems.Contains(itemid))
            {
                string payload = new JObject(new JProperty("value", value), new JProperty("type", "bw"), new JProperty("itemid", itemid), new JProperty("time", DateTime.Now.ToString())).ToString();

                Dictionary<string, string> dictObj = config["subscriptions"].ToObject<Dictionary<string, string>>();

                foreach (string pub in dictObj.Keys)
                {
                    if (pub == "bw")
                    {
                        bwCommands.Publish(dictObj["bw"], config["primary-access-chain"].ToString(), payload);
                    }
                }

                bwItems.Add(itemid);
            }
            else
            {
                bwItems.Remove(itemid);
            }
        }

        private void KNXItemValueChanged(object tag, IItemFacade facade)
        {
            int value = Int32.Parse(facade.GetValue().ToString());
            string itemid = facade.ItemId.ToString();

            host.WriteLog(nxaXIO.PlugKit.Logging.LogLevel.Warning, itemid + " : " + value + " changed.");

            Publish(value, itemid);
        }
 

        private void Publish(int value, string itemid)
        {
            try
            {
                string payload = new JObject(new JProperty("value", value), new JProperty("itemid", itemid), new JProperty("level", "inside")).ToString();

                Dictionary<string, string> dictObj = config["publishings"].ToObject<Dictionary<string, string>>();

                foreach (string pub in dictObj.Keys)
                {
                    if (pub == "knx")
                    {
                        bwCommands.Publish(dictObj["knx"], config["primary-access-chain"].ToString(), payload);
                    }
                }
                
                PlugLog.memoryLog.Add("KNX-ITEMCHANGED, " + value + ", " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ", " + itemid);

            }
            catch (Exception e)
            {
                host.WriteLog(nxaXIO.PlugKit.Logging.LogLevel.Error, e.Message + "\n" + e);
            }
        }
    }
}
