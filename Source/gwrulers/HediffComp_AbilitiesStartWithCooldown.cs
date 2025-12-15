using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace gwrulers
{
    public class HediffComp_AbilitiesStartWithCooldown : HediffComp
    {
        public HediffCompProperties_AbilitiesStartWithCooldown Props => (HediffCompProperties_AbilitiesStartWithCooldown)props;

        public override void CompPostMake()
        {
            base.CompPostMake();

            foreach (Ability ability in parent.AllAbilitiesForReading)
            {
                //ability.RemainingCharges = 0;
                ability.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
            }
        }
    }
}
