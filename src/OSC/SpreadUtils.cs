﻿using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Utils
{
    public static class SpreadUtils
    {
        public static bool IsAnyInvalid(this ISpread spread, params ISpread[] spreads)
        {
            if (spread == null) return true;

            if (spread.SliceCount == 0 || spread[0] == null) return true;
            else
            {
                for (int i = 0; i < spreads.Length; i++)
                {
                    if (spreads[i] == null) return true;
                    if (spreads[i].SliceCount == 0 || spreads[i][0] == null) return true;
                }
            }

            return false;
        }

    }
}