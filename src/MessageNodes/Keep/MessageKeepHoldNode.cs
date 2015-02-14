﻿using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using System.ComponentModel.Composition;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Hold", Category = "Message.Keep", Help = "Holds the last bunch of Messages that traveled through. Pick some or all.",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepHoldNode : AbstractMessageKeepNode
    {
        public enum HoldEnum
        {
            All,
            Index
        }

#pragma warning disable 649, 169

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true, Order = 2)]
        IDiffSpread<HoldEnum> FHold;

        [Input("Index", Order = int.MaxValue)]
        IDiffSpread<int> FIndex;

        [Output("Internal Count", Order = int.MaxValue)]
        ISpread<int> FCountOut;

#pragma warning restore

        public override void Evaluate(int SpreadMax)
        {
            var update = false;

            //var pin = (FType as Pin<EnumEntry>).PluginIO;
            //FType.Changed -= HandleTypeChange;
            //pin.PluginHost.DeletePin(pin);

            if (!FReset.IsAnyInvalid() && FReset[0])
            {
                Keep.Clear();
                update = true;
            }
            
            if (FInput.IsChanged && !FInput.IsAnyInvalid())
            {
                Keep.AssignFrom(FInput.Distinct()); // No need to hold duplicates
                FCountOut[0] = Keep.Count;
                update = true;
            }

            if (FHold.IsChanged ) update = true;
            if (FHold[0] == HoldEnum.Index && FIndex.IsChanged) update = true;

            if (update) 
            {
                switch (FHold[0])
                {
                    case HoldEnum.All:
                        FOutput.SliceCount = 0;
                        FOutput.AssignFrom(Keep);
                        break;
                    case HoldEnum.Index:
                        FOutput.SliceCount = FIndex.SliceCount;
                        for (int i = 0; i < FIndex.SliceCount;i++ )
                        {
                            FOutput[i] = Keep[FIndex[i]%Keep.Count];
                        }
                        break;
                }
                FOutput.Flush();
            }
         }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
           // do nothing.
        }
    }
}