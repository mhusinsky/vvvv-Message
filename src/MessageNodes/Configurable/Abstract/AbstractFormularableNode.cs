using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = "VVVV.Packs.Message.Core.Formular", Order = 2)]
        public IDiffSpread<EnumEntry> FFormular;
        protected bool FirstFrame = true;

        [Import]
        protected IHDEHost FHDEHost;

        protected override void InitializeWindow()
        {
            // don't call inherited InitializeWindow, so the placeholder pic will disappear
            
            FWindow = new PinDefinitionPanel();
            Controls.Add(FWindow);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            // base provider of Formulars
            var reg = MessageFormularRegistry.Instance;
            reg.TypeChanged += FormularChanged;

            // dummy enum, will be populated from registry
            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());


            // defer usage of all Formular related config to the end of the frame, when all nodes have finished at least once
            FHDEHost.MainLoop.OnRender += InitConfig;

            // events are always deferred to end of frame, so all potential participators have finished
            FFormular.Changed += (e) => FHDEHost.MainLoop.OnRender += HandleChangeOfFormular;
            ((PinDefinitionPanel)FWindow).OnChange += (s, e) => FHDEHost.MainLoop.OnRender += HandleChangeInWindow;
        }


        #region node formular update during runtime
        private void InitConfig(object sender, System.EventArgs e)
        {
            FHDEHost.MainLoop.OnRender -= InitConfig;

            SetWindowFromConfig();
            SetWindowFromRegistry();
        }

        private void HandleChangeOfFormular(object sender, System.EventArgs e)
        {
            FirstFrame = false;
            FHDEHost.MainLoop.OnRender -= HandleChangeOfFormular;

            SetWindowFromRegistry();
        }

        private void HandleChangeInWindow(object sender, System.EventArgs e)
        {
            FHDEHost.MainLoop.OnRender -= HandleChangeInWindow;
            var config = GetConfigurationFromWindow();
            FConfig[0] = config;
        }

        private void FormularChanged(MessageFormularRegistry sender, MessageFormularChangedEvent e)
        {
            if (FFormular.IsAnyInvalid()) return;

            var used = false;

            foreach (var type in FFormular)
                if (type.Name == e.Formular.Name) used = true;

            if (used) SetWindowFromRegistry();
        }
        #endregion node update during runtime



        #region update gui
        public virtual IList<MessageFormular> SetWindowFromConfig()
        {
            var formular = new MessageFormular(FConfig[0]);

            var forms = SetFormular(formular, true);
            return forms;

        }

        public virtual IList<MessageFormular> SetWindowFromRegistry()
        {
            if (FFormular.IsAnyInvalid() || FirstFrame || FFormular[0]==MessageFormular.DYNAMIC) return SetWindowFromConfig();

            var form = FFormular[0].Name;
            var formular = MessageFormularRegistry.Instance[form];

            var forms = SetFormular(formular);
            return forms;

        }

        protected virtual IList<MessageFormular> SetFormular(MessageFormular formular, bool forceChecked = false) 
        {
            var forms = new List<MessageFormular>();
                       
            var pinDef = FWindow as PinDefinitionPanel;
            pinDef.LayoutByFormular(formular, forceChecked);
            forms.Add(formular);
                
            return forms; //returns the Formulars.
        }
        #endregion update gui

        #region utils
        private string GetConfigurationFromWindow()
        {
            var window = FWindow as PinDefinitionPanel;
            var desc = window.CheckedDescriptors;
            var result = string.Join(", ", desc.Select(d => d.ToString()));
            return result;
        }
        #endregion utils

    }
}