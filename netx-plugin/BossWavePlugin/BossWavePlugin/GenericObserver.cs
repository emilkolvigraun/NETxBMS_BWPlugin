using nxaXIO.PlugKit.Nodes;

namespace BossWavePlugin
{
    public class GenericObserver : ItemObserver
    {
        public delegate void ValueChangeDelegate(object tag, IItemFacade facade);

        protected ValueChangeDelegate handler;

        public GenericObserver(ValueChangeDelegate handler)
        {
            this.handler = handler;
        }

        public override bool ItemValueChanged(object tag, IItemFacade itemFacade)
        {
            handler?.Invoke(tag, itemFacade);
            return true;
        }
    }
}
