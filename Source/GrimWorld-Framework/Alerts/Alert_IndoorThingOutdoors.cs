using System.Collections.Generic;
using System.Linq;
using GW_Frame.Comps.ThingComps;
using RimWorld;
using Verse;

namespace GW_Frame.Alerts
{
	public class Alert_IndoorThingOutdoors : Alert
	{
		private List<Thing> indoorsOutdoorsResult = new List<Thing>();

		public Alert_IndoorThingOutdoors()
		{
			defaultPriority = AlertPriority.High;
		}

		private List<Thing> IndoorsOutdoors
		{
			get
			{
				indoorsOutdoorsResult.Clear();
				foreach (var thing in Find.Maps.Where(map => map.mapPawns.AnyColonistSpawned).SelectMany(map => map.listerThings.GetAllThings()))
				{
					if (!thing.TryGetComp(out CompMustBeIndoors indoors)) continue;
					if (indoors.ShouldAlertNow) indoorsOutdoorsResult.Add(thing);
				}
				return indoorsOutdoorsResult;
			}
		}

		public override AlertReport GetReport() => AlertReport.CulpritsAre(IndoorsOutdoors);

		public override string GetLabel()
		{
			return "GW_IndoorsThingsOutdoorsLabel".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "GW_IndoorsThingsOutdoorsDesc".Translate();
		}
	}
}