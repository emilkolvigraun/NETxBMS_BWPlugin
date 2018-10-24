using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using nxaXIO.PlugKit.Logging;
using nxaXIO.PlugKit.Nodes;
using nxaXIO.PlugKit.Nodes.Requests;
using nxaXIO.PlugKit.Plugin;
using nxaXIO.PlugKit.Plugin.Results;
using BossWavePlugin.Host;
using Newtonsoft.Json.Linq;
using BWBinding;
using BWBinding.Common;
using BWBinding.Utils;
using System.Linq;
using System.Text;
using System.Threading;

namespace BossWavePlugin
{
    [PluginInfo("BossWavePlugin","BossWave Binding","1.0.0")]
    public class BossWavePlugin : PluginBase
    {
        public IPluginHost host;
        public static BossWavePlugin Instance { get; protected set; } // Using a singleton instance to communicate
        protected WebHost bwSignalR = new WebHost();

        private Observer itemObserver;

        private List<string> itemsChanged;

        public Dictionary<string, JObject> bwSubItems;
        public Dictionary<string, JObject> bwPubItems;
        public Dictionary<string, Thread> loopingPubItems;
        public List<string> entities;

        BossWaveClient bwClient;

        public JObject config;

        public BossWavePlugin()
        {
            Instance = this;
        }
      
        public override bool Init(uint data1, uint data2)
        {
            host = mHost_;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bwconfig.json";
            using (StreamReader r = new StreamReader(path))
            {
                config = JObject.Parse(r.ReadToEnd());
            }

            bwClient = new BossWaveClient("localhost", BWDefaults.DEFAULT_PORT_NUMBER);
            bwSubItems = new Dictionary<string, JObject>();
            bwPubItems = new Dictionary<string, JObject>();
            loopingPubItems = new Dictionary<string, Thread>();
            entities = new List<string>();
            itemsChanged = new List<string>();
            itemObserver = new Observer(BWItemValueChange);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            try
            {
                bwClient.Connect();
            } catch (Exception e)
            {
                mHost_.WriteLog(LogLevel.Error, e.Message);
            }
            return true;
        }

        static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (folderPath != null)
            {
                string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
                if (File.Exists(assemblyPath) == false) return null;
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            return null;
        }

        public override bool RegisterItems()
        {
            ItemCreateOptions sub_options = new ItemCreateOptions
            {
                Description = "Subscription",
                Datatype = typeof(string),
                AccessRights = ItemAccess.ReadWrite
            };

            ItemCreateOptions pub_options = new ItemCreateOptions
            {
                Description = "Publishing",
                Datatype = typeof(string),
                AccessRights = ItemAccess.ReadWrite
            };

            for (int i = 0; i < Int32.Parse(config["entity-items"].ToString()); i++)
            {
                mHost_.CreateItem(@"NETx\XIO\BossWave\Identity\Ent" + i, new ItemCreateOptions { Description = "Entity path", Datatype = typeof(string), AccessRights = ItemAccess.ReadWrite });
            }

            for (int i = 0; i < Int32.Parse(config["sub-items"].ToString()); i++)
            {
                var subItem = mHost_.CreateItem(@"NETx\XIO\BossWave\Subscriptions\Sub" + i, sub_options);
                subItem.ItemFacade.RegisterObserver(itemObserver);
            }

            for (int i = 0; i < Int32.Parse(config["pub-items"].ToString()); i++)
            {
                var pubItem = mHost_.CreateItem(@"NETx\XIO\BossWave\Pulishes\Pub" + i, pub_options);
                pubItem.ItemFacade.RegisterObserver(itemObserver);
            }

            // creating mockup KNX item tree
            mHost_.CreateItem(@"NETx\XIO\KNX\001", new ItemCreateOptions { Description = "Fictional radiator", Datatype = typeof(int), AccessRights = ItemAccess.ReadWrite });
            mHost_.CreateItem(@"NETx\XIO\KNX\011", new ItemCreateOptions { Description = "Fictional lighting", Datatype = typeof(int), AccessRights = ItemAccess.ReadWrite });
            mHost_.CreateItem(@"NETx\XIO\KNX\111", new ItemCreateOptions { Description = "Fictional server", Datatype = typeof(int), AccessRights = ItemAccess.ReadWrite });

            foreach (var item in mHost_.GetBranch(@"NETx\XIO\KNX").BranchFacade.GetItems())
            {
                item.RegisterObserver(itemObserver);
            }

            return true;
        }

        private void LoadBWItems()
        {
            foreach (var item in mHost_.GetBranch(@"NETx\XIO\BossWave\Subscriptions").BranchFacade.GetItems())
            {
                if (item.GetValue().ToString().Length == 0)
                {
                    JObject jObj = JObject.Parse(item.GetValue().ToString());
                    string title = jObj["title"].ToString();
                    bwSubItems.Add(title, jObj);
                }
            }
            foreach (var item in mHost_.GetBranch(@"NETx\XIO\BossWave\Pulishes").BranchFacade.GetItems())
            {
                if (item.GetValue().ToString().Length == 0)
                {
                    JObject jObj = JObject.Parse(item.GetValue().ToString());
                    string title = jObj["title"].ToString();
                    bwPubItems.Add(title, jObj);
                }
            }
        }

        public override bool Start()
        {
            try
            {
                bwSignalR.Start();
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Start -> Ex: " + ex);
            }
            return true;
        }

        public override bool Stop()
        {
            try
            {
                bwSignalR.Stop();
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Start -> Ex: " + ex);
            }
            return true;
        }

        public override bool ShutDown()
        {
            try
            {
                bwSignalR.Stop();
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Start -> Ex: " + ex);
            }
            return true;
        }

        public override void StateChange(BmsServerState oldState, BmsServerState newState){ }

        public bool CreateSubItem(string info)
        {
            foreach (var item in mHost_.GetBranch(@"NETx\XIO\BossWave\Subscriptions").BranchFacade.GetItems())
            {
                if (item.GetValue().ToString().Length == 0)
                {
                    JObject jObj = JObject.Parse(info);
                    string title = jObj["title"].ToString();
                    bwSubItems.Add(title, jObj);
                    Subscribe(jObj);
                    item.SetValue(new UpdateRequest(info, ItemChangeReason.IoReceived));
                    break;
                }
            }
            return true;
        }

        private void Subscribe(JObject info)
        {
            try
            {
                bool autochain = Boolean.Parse(info["autochain"].ToString());
                int expirydelta = Int32.Parse(info["expirydelta"].ToString());
                string ns = info["namespace"].ToString();

                Request subscribeRequest = new RequestUtils(ns, RequestType.SUBSCRIBE)
                    .SetExpiryDelta(expirydelta)
                    .SetAutoChain(autochain)
                    .BuildSubcriber();

                bwClient.Subscribe(subscribeRequest, new ResponseHandler("Subscription to " + info["namespace"].ToString()), new MessageHandler());
            } catch (Exception e)
            {
                mHost_.WriteLog(LogLevel.Error, e.Message);
            }
        }

        public bool CreatePubItem(string info)
        {
            foreach (var item in mHost_.GetBranch(@"NETx\XIO\BossWave\Pulishes").BranchFacade.GetItems())
            {
                if (item.GetValue().ToString().Length == 0)
                {
                    JObject jObj = JObject.Parse(info);
                    string title = jObj["title"].ToString();

                    if (Boolean.Parse(jObj["loop"].ToString()))
                    {
                        mHost_.WriteLog(LogLevel.Warning, jObj["loop"].ToString());
                        loopingPubItems.Add(jObj["title"].ToString(), new Thread(() => PublishLooper(jObj)));
                        mHost_.WriteLog(LogLevel.Warning, "added thread");
                        loopingPubItems[jObj["title"].ToString()].Start();
                        mHost_.WriteLog(LogLevel.Warning, "thread started");
                    } 

                    bwPubItems.Add(title, jObj);
                    item.SetValue(new UpdateRequest(info, ItemChangeReason.IoReceived));
                    break;
                }
            }
            return true;
        }

        private void PublishLooper(JObject info)
        {
            while (true)
            {
                try
                {
                    RequestUtils publishRequestUtils = new RequestUtils(info["namespace"].ToString(), RequestType.PUBLISH).SetPrimaryAccessChain(info["chain"].ToString());

                    mHost_.WriteLog(LogLevel.Warning, "thread running");
                    byte[] message = Encoding.UTF8.GetBytes(info["message"].ToString());

                    byte[] text = { 64, 0, 0, 0 };

                    PayloadObject payload = new PayloadObject(new PayloadType(text), message);
                    publishRequestUtils.AddPayloadObject(payload);
                    Request publishRequest = publishRequestUtils.BuildPublisher();

                    bwClient.Publish(publishRequest, new ResponseHandler("publish"));

                    Thread.Sleep(Int32.Parse(info["ms"].ToString())); // sleep the stated amount of ms
                }
                catch (Exception e)
                {
                    mHost_.WriteLog(LogLevel.Error, e.Message);
                }
            }
        }

        private void Publish(JObject info)
        {
            try
            {
                RequestUtils publishRequestUtils = new RequestUtils(info["namespace"].ToString(), RequestType.PUBLISH).SetPrimaryAccessChain(info["primary-access-chain"].ToString());

                byte[] message = Encoding.UTF8.GetBytes(info["message"].ToString());

                byte[] text = { 64, 0, 0, 0 };

                PayloadObject payload = new PayloadObject(new PayloadType(text), message);
                publishRequestUtils.AddPayloadObject(payload);
                Request publishRequest = publishRequestUtils.BuildPublisher();

                bwClient.Publish(publishRequest, new ResponseHandler("publish"));
            }catch(Exception e)
            {
                mHost_.WriteLog(LogLevel.Error, e.Message);
            }
        }

        public bool SetEntity(string path)
        {      
            try
            {
                foreach (var entity in mHost_.GetBranch(@"NETx\XIO\BossWave\Identity").BranchFacade.GetItems())
                {
                    if (entity.GetValue().ToString().Length == 0)
                    {
                        if(entities.ToArray().Length == 0 || !entities.Any(item => entity.GetValue().ToString() == path))
                        {
                            entities.Add(path);
                            entity.SetValue(new UpdateRequest(path, ItemChangeReason.IoReceived));
                            bwClient.SetEntity(path, new ResponseHandler("SetEntity()"));
                            break;
                        } else
                        {
                            return true;
                        } 
                    }
                }
            } catch (Exception e)
            {
                mHost_.WriteLog(LogLevel.Error, e.Message);
            }

            return true;
        }

        private void BWItemValueChange(object tag, IItemFacade facade)
        {
            try
            {
                var itemId = facade.ItemId;
                var newvalue = facade.GetValue(); 
                mHost_.WriteLog(LogLevel.Warning, "changed:" + itemId + " " + newvalue);
                JObject jObj = JObject.Parse(newvalue.ToString());
                string title = jObj["title"].ToString();
                if (bwSubItems.ContainsKey(title))
                {
                    bwSubItems[title] = jObj;
                }
                if (bwPubItems.ContainsKey(title))
                {
                    bwPubItems[title] = jObj;
                }
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Start -> Ex: " + ex);
            }
        }
    }
}
