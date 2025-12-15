using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace gwrulers
{
    public class HediffCompProperties_AbilitiesStartWithCooldown : HediffCompProperties
    {
        public HediffCompProperties_AbilitiesStartWithCooldown()
        {
            compClass = typeof(HediffComp_AbilitiesStartWithCooldown);
        }
    }
}
