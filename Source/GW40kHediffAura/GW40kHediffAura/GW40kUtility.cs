using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GW40kHediffAura;

public class GW40kUtility
{
	public static List<Pawn> GetNearbyPawnFriendAndFoe(IntVec3 center, Map map, float radius)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		List<Pawn> list = new List<Pawn>();
		float num = radius * radius;
		foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
		{
			if (((Thing)item).Spawned && !item.Dead)
			{
				float num2 = IntVec3Utility.DistanceToSquared(((Thing)item).Position, center);
				if (num2 <= num)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public static float GetSeverityAfterModifier(Pawn pawn, float severity, List<StatDef> statDefs = null)
	{
		float num = severity;
		float num2 = num;
		if (!GenList.NullOrEmpty<StatDef>((IList<StatDef>)statDefs))
		{
			foreach (StatDef statDef in statDefs)
			{
				if (statDef != StatDefOf.ToxicResistance && statDef != StatDefOf.ToxicEnvironmentResistance)
				{
					num2 *= StatExtension.GetStatValue((Thing)(object)pawn, statDef, true, -1);
				}
			}
			if (statDefs.Contains(StatDefOf.ToxicEnvironmentResistance))
			{
				num2 += num2 * (0f - StatExtension.GetStatValue((Thing)(object)pawn, StatDefOf.ToxicEnvironmentResistance, true, -1));
			}
			if (statDefs.Contains(StatDefOf.ToxicResistance))
			{
				num2 += num2 * (0f - StatExtension.GetStatValue((Thing)(object)pawn, StatDefOf.ToxicResistance, true, -1));
			}
			num = num2;
		}
		return num;
	}

	public static Hediff CreateHediff(HediffDef hd, Pawn pawn, float sev)
	{
		Hediff val = HediffMaker.MakeHediff(hd, pawn, (BodyPartRecord)null);
		val.Severity = sev;
		return val;
	}
}
