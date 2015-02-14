using System.ComponentModel.Composition;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using System.Linq;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Autolearn Type", IsSingle = true, DefaultBoolean = true, IsToggle = true)]
        public IDiffSpread<bool> FAutoLearnMode;
        
        [Input("Message Formular", DefaultEnumEntry = "None", IsSingle = true, EnumName = "VVVV.Packs.Message.Core.Formular", Order = 2)]
        public IDiffSpread<EnumEntry> FType;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            FType.Changed += HandleTypeChange;
            FAutoLearnMode.Changed += HandleLearnModeChange;

            var reg = MessageFormularRegistry.Instance;
            reg.TypeChanged += ConfigChanged;

            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());
        }

        protected virtual void HandleLearnModeChange(IDiffSpread<bool> spread)
        {
            SetFormular();
        }

        protected virtual void HandleTypeChange(IDiffSpread<EnumEntry> spread)
        {
            SetFormular();
        }

        protected void SetFormular() 
        {
            if (!FAutoLearnMode[0] || FType.IsAnyEmpty()) return;

            var form = FType[0].Name;
            if (form != MessageFormular.DYNAMIC) FConfig[0] = MessageFormularRegistry.Instance[form].ToString(true);
        }

        protected virtual void ConfigChanged(MessageFormularRegistry sender, MessageFormularChangedEvent e)
        {
            if (!FType.IsAnyEmpty() && e.Formular.Name == FType[0].Name) SetFormular();
        }

  
    }
}