using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;
using Verse.AI;

namespace gwrulers
{
    public class CompAbilityEffect_EmperorLOTD : CompAbilityEffect
    {
        private new CompProperties_AbilityEmperorLOTD Props => (CompProperties_AbilityEmperorLOTD)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            int num = Mathf.Max(1, Mathf.FloorToInt(parent.pawn.psychicEntropy.PsychicSensitivity / Props.psychicSensitivityExtraSummmonFactor));

            HediffDef temporaryHediff;
            switch (num)
            {
                case 1:
                    temporaryHediff = HediffDef.Named("GW_Ruler_EmperorLOTD_6Hours");
                    break;
                case 2:
                    temporaryHediff = HediffDef.Named("GW_Ruler_EmperorLOTD_12Hours");
                    break;
                default:
                    temporaryHediff = HediffDef.Named("GW_Ruler_EmperorLOTD_24Hours");
                    break;
            }

            PawnKindDef kindDef = PawnKindDef.Named(Props.pawnKindDef);

            Faction faction = Faction.OfPlayer;
            //Faction faction = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed(Props.factionDef));

            if (faction == null)
            {
                Messages.Message("The " + faction.def.LabelCap + " faction tried to heed your summon, but failed.", MessageTypeDefOf.NeutralEvent);
                return;
            }

            if (faction.HostileTo(Faction.OfPlayer))
            {
                Messages.Message("The " + faction.def.LabelCap + " faction refused to heed your summon.", MessageTypeDefOf.NeutralEvent);
                return;
            }

            for (int i = 0; i < num; i++)
            {
                Pawn newPawn = PawnGenerator.GeneratePawn(kindDef, faction);
                //newPawn.guest.SetGuestStatus(Faction.OfPlayer);
                GenSpawn.Spawn(newPawn, target.Cell, parent.pawn.Map);

                newPawn.apparel.LockAll();
                newPawn.health.AddHediff(temporaryHediff);
                Job newJob = JobMaker.MakeJob(JobDefOf.Follow, parent.pawn);
                newPawn.jobs.StartJob(newJob, JobCondition.Incompletable);
            }

            base.Apply(target, dest);
        }
    }
}
