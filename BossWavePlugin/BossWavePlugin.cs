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

namespace BossWavePlugin
{
    [PluginInfo("BossWavePlugin","BossWave Binding","1.0.0")]
    public class BossWavePlugin : PluginBase
    {

        public class MyItemObserver : ItemObserver
        {
            public delegate  void ValueChangeDelegate(object tag, IItemFacade facade);

            protected ValueChangeDelegate mHandler_;
            public MyItemObserver(ValueChangeDelegate handler)
            {
                mHandler_ = handler;    
            }

            public override bool ItemValueChanged(object tag, IItemFacade itemFacade)
            {
                if(mHandler_!= null)
                {
                    mHandler_(tag, itemFacade);
                }
                return true;
            }
        }


        readonly MyItemObserver mItemObserver__;

        public static BossWavePlugin Instance
        {
            get; protected set; 
            
        }
        public BossWavePlugin()
        {
            Instance = this;
            mItemObserver__ = new MyItemObserver(ItemValueChange);
        }
        protected WebHost mSignalr_ = new WebHost();
        protected SortedList<string, IItemFacade> mItems_ = new SortedList<string, IItemFacade>();
      
        public override bool Init(uint data1, uint data2)
        {
            /*
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
                Debugger.Break();
            }*/
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;
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
            var br = mHost_.GetBranch(@"NETx\XIO\PlugMan");
            if (br.ResultCode == GetBranchesultCodes.OK)
            {
                RegisterTree(br.BranchFacade);

            }
            return true;
        }

        public override bool Start()
        {
            try
            {
                mSignalr_.Start();
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
                mSignalr_.Stop();
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
                mSignalr_.Stop();
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Start -> Ex: " + ex);
            }

            return true;
        }

        public override void StateChange(BmsServerState oldState, BmsServerState newState)
        {
            
        }


        protected object mSetLock_ = new object();
        public void SetItemValue(string itemId, string itemValue)
        {
            if(string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(itemValue))
            {
                // ActionHub.Announce("ItemID or Value is empty");
                return;
            }
            lock(mSetLock_)
            {
                
                IItemFacade valueFacade;
                if(!mItems_.ContainsKey(itemId))
                {
                    var cres = mHost_.GetItem(itemId);
                    if(cres.ResultCode == GetItemResultCodes.OK)
                    {                       
                        RegisterItem(cres.ItemFacade);                        
                    }
                }
                if(mItems_.TryGetValue(itemId,out valueFacade))
                {
                    valueFacade.SetValue(new UpdateRequest(itemValue, ItemChangeReason.IoReceived));
                }
                else
                {
                    // ActionHub.Announce("ItemID not valid:" + itemId);
                }
            }
        }

        protected object mExecScriptLock_ = new object();
        public void ExecLuaScript(string script)
        {
            if(string.IsNullOrEmpty(script))
            {
                // ActionHub.Announce("Can't execute empty script.");
                return;
            }
           var res =  mHost_.ExecuteLuaScript(script);
            // ActionHub.Announce("ExecuteScript result" + res.ToString());
        }

        protected void RegisterTree(IBranchFacade branch)
        {
            if (branch != null)
            {
                var items = branch.GetItems();
                foreach (var item in items)
                {
                    RegisterItem(item);
                }
                var branches = branch.GetBranches();
                foreach (var subBranch in branches)
                {
                    RegisterTree(subBranch);
                }
            }
        }

    
        protected void RegisterItem(IItemFacade item)
        {
           
            if (item != null)
            {
                item.RegisterObserver(mItemObserver__);
                mItems_[item.ItemId]=item;
            }
        }


        private void ItemValueChange(object tag, IItemFacade facade)
        {
            try
            {
                var itemId = facade.ItemId;
                var newvalue = facade.GetValue();
                string valStr;
                try
                {
                    valStr = Convert.ToString(newvalue);
                }
                catch (Exception)
                {
                    valStr = "[Parse Err]";
                }
                // ActionHub.Announce(itemId + valStr);
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Start -> Ex: " + ex);
            }
        }
    }
}
