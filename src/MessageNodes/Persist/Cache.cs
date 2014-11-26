﻿
#region usings
using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.Linq;
using VVVV.Packs.Message.Core;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Packs.Message.Nodes
{
    [PluginInfo(Name = "Cache",
        Category = "Message",
        AutoEvaluate = true,
        Help = "Stores Messages and removes them, if no change was detected for a certain time",
        Author = "velcrome")]
    public class CacheNode : AbstractStorageNode
    {
        #region fields & pins

        [Input("Retain Time", IsSingle = true, DefaultValue = -1.0, Order = 3)]
        public ISpread<double> FTime;

        [Output("Removed Messages", AutoFlush = false, Order = 2)]
        public ISpread<Core.Message> FRemovedMessages;

        #endregion fields & pins

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            
            if (FReset[0])
            {
                data.Clear();
            }



            List<bool> changed = Match();

            if (FTime[0] > 0)
            {
                var validTime = Time.Time.CurrentTime() -
                                new TimeSpan(0, 0, 0, (int) Math.Floor(FTime[0]), (int) Math.Floor((FTime[0]*1000)%1000));

                var clear = (from message in data
                            where message.TimeStamp < validTime
                            select message).ToArray();

                foreach (var m in clear)
                {
                    var index = data.IndexOf(m);
                    data.RemoveAt(index);
                    changed.RemoveAt(index);
                }

                if (FRemovedMessages.SliceCount > 0 || clear.Length != 0)
                {
                    FRemovedMessages.SliceCount = 0;
                    FRemovedMessages.AssignFrom(clear);
                    FRemovedMessages.Flush();
                }
                else
                {
                    // still empty, no need to flush
                }

            }
            FOutput.SliceCount = 0;
            FOutput.AssignFrom(data);

            FChanged.AssignFrom(changed);


        }
    }


}
