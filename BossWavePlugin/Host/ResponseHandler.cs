using BWBinding.Common;
using BWBinding.Interfaces;
using nxaXIO.PlugKit.Logging;

namespace BossWavePlugin.Host
{
    class ResponseHandler : IResponseHandler
    {
        public Response result { get; set; }

        public bool received { get; set; }

        private string context;

        public ResponseHandler(string context)
        {
            this.context = context;
        }

        public void ResponseReceived(Response result)
        {
            BossWavePlugin.Instance.host.WriteLog(LogLevel.Warning, context + " : " + result.status + " : " + result.reason);
        }
    }
}
