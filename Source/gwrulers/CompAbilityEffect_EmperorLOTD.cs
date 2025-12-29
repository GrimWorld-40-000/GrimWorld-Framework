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
            int num = ModsConfig.RoyaltyActive ? Mathf.Max(1, Mathf.FloorToInt(parent.pawn.psychicEntropy.PsychicSensitivity / Props.psychicSensitivityExtraSummmonFactor)) : Props.defaultScaling;

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
                float chanceSum = 0f;
                foreach (PawnKindDef_Percentage p in Props.pawnKindDefs)
                {
                    chanceSum += p.chance;
                }

                PawnKindDef pawnKindDef = Props.pawnKindDefs.LastOrDefault()?.pawnKindDef;

                float roll = Rand.Value;
                float weight = 0f;
                foreach (PawnKindDef_Percentage p in Props.pawnKindDefs)
                {
                    weight += p.chance;
                    if (roll < weight / chanceSum)
                    {
                        pawnKindDef = p.pawnKindDef;
                        break;
                    }
                }

                Pawn newPawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
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
