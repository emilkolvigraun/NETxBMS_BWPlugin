using nxaXIO.PlugKit.Plugin;
using nxaXIO.PlugKit.Plugin.Results;
using System;

namespace BossWave
{
    [PluginInfo("BossWave", "A NETx BMS plugin for BOSSWAVE 2.2.0", "1.0.0")]
    public class Main : PluginBase
    {
        public override bool Init(uint data1, uint data2)
        {
            throw new NotImplementedException();
        }

        public override bool RegisterItems()
        {
            throw new NotImplementedException();
        }

        public override bool ShutDown()
        {
            throw new NotImplementedException();
        }

        public override bool Start()
        {
            throw new NotImplementedException();
        }

        public override void StateChange(BmsServerState oldState, BmsServerState newState)
        {
            throw new NotImplementedException();
        }

        public override bool Stop()
        {
            throw new NotImplementedException();
        }
    }
}
