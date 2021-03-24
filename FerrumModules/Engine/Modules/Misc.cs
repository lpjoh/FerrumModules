using System;
using System.Collections.Generic;

namespace Crossfrog.Ferrum.Engine.Modules
{
    public static class Misc
    {
        public static int SignConsciousModulus(int value, int divisor)
        {
            if (value >= 0) return value % divisor;
            return (divisor - (-value % divisor)) % divisor;
        }
        public static float NormalizedByte(byte value) => ((float)value + 1) / 256;
        public static List<BaseType> OnlyWithBase<BaseType, ListType>(List<ListType> list) where BaseType : ListType
        {
            var baseList = new List<BaseType>();
            foreach (var element in list)
                if (typeof(BaseType).IsAssignableFrom(element.GetType()))
                    baseList.Add((BaseType)element);

            return baseList;
        }
    }
}
