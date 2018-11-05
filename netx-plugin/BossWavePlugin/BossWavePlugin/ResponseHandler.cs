using BWBinding.Common;
using BWBinding.Interfaces;
using System;

namespace BossWavePlugin
{
    class ResponseHandler : IResponseHandler
    {
        public Response result { get; set; }

        public bool received { get; set; }

        private string msg;

        public ResponseHandler(string msg)
        {
            this.msg = msg;
        }

        public void ResponseReceived(Response result)
        {
            PlugLog.memoryLog.Add("RESPONSE, " + msg + ", " + result.status + ", " + result.reason + ", " + DateTime.Now.ToString());
        }
    }
}
