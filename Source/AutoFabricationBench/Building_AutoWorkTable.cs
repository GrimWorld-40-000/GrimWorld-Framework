using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace AutoFabricationBench
{
    public class Building_AutoWorkTable : Building_WorkTableAutonomous
    {
        public Building_AutoWorkTable()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        private Sustainer workingSound;

        private CompPowerTrader power;

        public CompPowerTrader Power
        {
            get
            {
                if (power == null)
                {
                    power = this.TryGetComp<CompPowerTrader>();
                }

                return power;
            }
        }

        public bool PoweredOn => Power.PowerOn;

        //public override void Notify_StartForming(Pawn billDoer)
        //{
            //while (activeBill.State < FormingState.Forming)
            //{
            //    activeBill.Notify_BillWorkFinished(billDoer);
            //}
            //Log.Message("activeBill.State = " + activeBill.State.ToString());
            //Log.Message("activeBill.suspended = " + activeBill.suspended);
            //SoundDef.Named("AutoFabricationBench_Started").PlayOneShot(this);
        //}

        public override void Notify_FormingCompleted()
        {
            //List<Thing> products = new List<Thing>();
            //for (int i = 0; i < activeBill.recipe.products.Count; i++)
            //{
            //    products.Add(ThingMaker.MakeThing(activeBill.recipe.products[i].thingDef));
            //    products[i].stackCount = activeBill.recipe.products[i].count;
            //}
            Messages.Message("Auto Fabrication Complete" + ": " + activeBill.recipe.LabelCap, this, MessageTypeDefOf.PositiveEvent);
            innerContainer.ClearAndDestroyContents();
            //for (int i = 0; i < products.Count; i++)
            //{
            //    GenSpawn.Spawn(products[i], InteractionCell, Map);
            //}
            //SoundDef.Named("AutoFabricationBenchBill_Completed").PlayOneShot(this);
        }

        //public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        //{
        //    SoundDef.Named("AutoFabricationBench_MaterialInserted").PlayOneShot(this);
        //}

        protected override void Tick()
        {
            base.Tick();
            if (activeBill != null && PoweredOn)
            {
                activeBill.BillTick();
            }

            if (this.IsHashIntervalTick(250))
            {
                if (activeBill != null && activeBill.State == FormingState.Forming)
                {
                    Power.PowerOutput = 0f - Power.Props.PowerConsumption;
                }
                else
                {
                    Power.PowerOutput = 0f - Power.Props.idlePowerDraw;
                }
            }

            //if (activeBill != null && PoweredOn && activeBill.State != 0)
            //{
            //    if (workingSound == null || workingSound.Ended)
            //    {
            //        workingSound = SoundDef.Named("AutoFabricationBench_Ambience").TrySpawnSustainer(this);
            //    }

            //    workingSound.Maintain();
            //}
            else if (workingSound != null)
            {
                workingSound.End();
                workingSound = null;
            }
        }
    }
}
