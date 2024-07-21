using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace Chamber
{
    public class ChamberModExtension : DefModExtension
    {
        public GraphicData chamberTopGraphic;
        public int ticksToFinish;
        public float brainDamageChance;
        public float brainDamageSeverity;
    }
}
