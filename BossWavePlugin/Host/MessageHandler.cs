using BWBinding.Common;
using BWBinding.Interfaces;

namespace BossWavePlugin.Host
{
    class MessageHandler : IMessageHandler
    {
        public Message message { get; set; }

        public bool received { get; set; }

        public void ResultReceived(Message message)
        {
            // handle result
        }
    }
}
