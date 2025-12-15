using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace gwrulers
{
    class HediffComp_DespawnAfterHediffRemoved : HediffComp
    {
        private HediffCompProperties_DespawnAfterHediffRemoved Props => (HediffCompProperties_DespawnAfterHediffRemoved)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Pawn.DeSpawn();
        }
    }
}
