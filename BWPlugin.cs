using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using nxaXIO.PlugKit.Logging;
using nxaXIO.PlugKit.Nodes;
using nxaXIO.PlugKit.Nodes.Requests;
using nxaXIO.PlugKit.Plugin;
using nxaXIO.PlugKit.Plugin.Results;

namespace BWPlugin
{
    [PluginInfo("BWPlugin", "A plugin for BOSSWAVE 2.2.0", "1.0.0")]
    public class BWPlugin : PluginBase
    {
        protected List<ICreateItemResult> mItems_ = new List<ICreateItemResult>();

        public override bool Init(uint data1, uint data2)
        {
            return true;
        }

        public override bool RegisterItems()
        {
            var opts = new ItemCreateOptions
            {
                Description = "D1Item1",
                Datatype = typeof (string),
                AccessRights = ItemAccess.ReadWrite
            };
            try
            {
                var tmpres = mHost_.CreateTemplate("mytemplate");
                if (tmpres.ResultCode == NodeTemplateCreateResults.OK)
                {
                    var template = tmpres.Template;
                    template.CreateProp(7001, "DEMO", "DEMODEFAULT");
                    template.Construct();
                    opts.Template = template;

                }
            }
            catch (Exception ex)
            {
                mHost_.WriteLog(LogLevel.Error, "Template ex: " + ex);
            }


            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D1\D1\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D1\D2\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D2\D1\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D2\D1\D1\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D2\D2\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D2\D2\D2\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D3\D1\D1\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D3\D1\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D1\D3\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D3\D3\Item1", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D1\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D1\D2\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D2\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D2\D1\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D2\D2\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D2\D2\D2\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D3\D1\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D3\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D1\D3\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\XIO\PlugMan\D1\D1\D3\D3\Item2", opts));

            mItems_.Add(mHost_.CreateItem(@"NETx\Module\PlugMan\D1\D1\D2\D2\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\Module\PlugMan\D1\D2\D2\D2\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\Module\PlugMan\D1\D3\D1\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\Module\PlugMan\D1\D1\D3\D1\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\Module\PlugMan\D1\D1\D1\D3\Item2", opts));
            mItems_.Add(mHost_.CreateItem(@"NETx\Module\PlugMan\D1\D1\D3\D3\Item2", opts));
            /*
            try
            {
                mHost_.WriteLog(LogLevel.Message, "START Register in wrong path");
                mItems_.Add(mHost_.CreateItem(@"NETx\Server\PlugMan\D1\D1\D2\D2\Item2", opts));
                mItems_.Add(mHost_.CreateItem(@"NETx\Server\PlugMan\D1\D2\D2\D2\Item2", opts));
                mItems_.Add(mHost_.CreateItem(@"NETx\Server\PlugMan\D1\D3\D1\D1\Item2", opts));
                mItems_.Add(mHost_.CreateItem(@"NETx\Server\PlugMan\D1\D1\D3\D1\Item2", opts));
                mItems_.Add(mHost_.CreateItem(@"NETx\Server\PlugMan\D1\D1\D1\D3\Item2", opts));
                mItems_.Add(mHost_.CreateItem(@"NETx\Server\PlugMan\D1\D1\D3\D3\Item2", opts));
                mHost_.WriteLog(LogLevel.Message, "END Register in wrong path");
                
            }
            catch(Exception ex)
            {
                mHost_.WriteLog(LogLevel.Message,"ex: "+ex);
            }
            */
            mCommBackend_ = new SimpleCommunicationBackend(ReadHandler, WriteHandler, UpdateHandler);
            foreach (var createItemResult in mItems_)
            {
                if (createItemResult.ResultCode == CreateItemResultCodes.OK)
                {
                    createItemResult.ItemHost.RegisterStoreHandler(mCommBackend_);
                }
            }
            return true;
        }
        protected SimpleCommunicationBackend mCommBackend_;

        protected void UpdateTemplateProp()
        {
            var i = 0;
            foreach (var createItemResult in mItems_)
            {
                if (createItemResult.ResultCode == CreateItemResultCodes.OK)
                {
                    var prop = createItemResult.ItemFacade.GetProperty(7001);
                    if (prop != null)
                    {
                        i++;
                        prop.SetValue("PVal: " + i);
                        createItemResult.ItemFacade.SetProperty(prop);
                    }
                }
            }
        }

        private void UpdateHandler(IUpdateRequest request, object tag, IItemFacade itemfacade)
        {

        }

        private bool WriteHandler(IWriteRequest request, object tag, IItemFacade itemfacade)
        {
            itemfacade.SetValue(new UpdateRequest(request.Value(), ItemChangeReason.IoReceived));
            return true;
        }

        private bool ReadHandler(object tag, IItemFacade itemfacade)
        {
            return true;
        }

        Thread mThread__;
        bool mStopping__;
        public override bool Start()
        {
            UpdateTemplateProp();

            mThread__ = new Thread(ThreadWorker);
            mThread__.Start();
            return true;
        }
        protected void ThreadWorker()
        {
            mHost_.WriteLog(LogLevel.Warning, "Thread Start");
            while (!mStopping__)
            {
                try
                {
                    foreach (var createItemResult in mItems_)
                    {
                        if (createItemResult.ResultCode == CreateItemResultCodes.OK)
                        {
                            var tims = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                            createItemResult.ItemFacade.SetValue(new UpdateRequest(tims, ItemChangeReason.IoReceived));
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                if (!mStopping__)
                {
                    Thread.Sleep(2000);
                }
            }
            mHost_.WriteLog(LogLevel.Warning, "Thread END");
        }
        public override bool Stop()
        {
            mStopping__ = true;
            return true;
        }

        public override bool ShutDown()
        {
            return true;
        }

        public override void StateChange(BmsServerState oldState, BmsServerState newState)
        {

        }
    }
}
