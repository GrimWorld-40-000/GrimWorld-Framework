using System.Collections.Generic;
using Verse;

namespace GW40kHediffAura;

public class Comp_ApparelGiveHediff : ThingComp
{
	public CompProperties_ApparelGiveHediff Props => (CompProperties_ApparelGiveHediff)(object)base.props;

	public override void PostExposeData()
	{
		((ThingComp)this).PostExposeData();
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		((ThingComp)this).Notify_Equipped(pawn);
		if (pawn?.health?.hediffSet == null || Props?.hediffDef == null)
		{
			return;
		}

		PendingEquipHediffApplications.Register(pawn, Props.hediffDef);
	}

	internal static void TryAddHediff(Pawn pawn, HediffDef hediffDef)
	{
		if (pawn?.health?.hediffSet == null || hediffDef == null || pawn.health.hediffSet.HasHediff(hediffDef, false))
		{
			return;
		}

		Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, (BodyPartRecord)null);
		if (hediff == null)
		{
			return;
		}

		hediff.Severity = 1f;
		pawn.health.AddHediff(hediff);
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		((ThingComp)this).Notify_Unequipped(pawn);
		if (pawn?.health?.hediffSet == null || Props?.hediffDef == null)
		{
			return;
		}

		PendingEquipHediffApplications.Unregister(pawn, Props.hediffDef);
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef, false);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
	}
}

internal static class PendingEquipHediffApplications
{
	private static readonly List<PendingApplication> Pending = new();

	public static void Register(Pawn pawn, HediffDef hediffDef)
	{
		if (pawn == null || hediffDef == null || Contains(pawn, hediffDef))
		{
			return;
		}

		Pending.Add(new PendingApplication(pawn, hediffDef));
	}

	public static void Unregister(Pawn pawn, HediffDef hediffDef)
	{
		Pending.RemoveAll(application => application.Pawn == pawn && application.HediffDef == hediffDef);
	}

	public static void Tick()
	{
		for (int i = Pending.Count - 1; i >= 0; i--)
		{
			PendingApplication application = Pending[i];
			Pawn pawn = application.Pawn;
			if (pawn == null || pawn.Destroyed || pawn.Dead)
			{
				Pending.RemoveAt(i);
				continue;
			}

			if (!pawn.Spawned)
			{
				continue;
			}

			Comp_ApparelGiveHediff.TryAddHediff(pawn, application.HediffDef);
			Pending.RemoveAt(i);
		}
	}

	private static bool Contains(Pawn pawn, HediffDef hediffDef)
	{
		return Pending.Exists(application => application.Pawn == pawn && application.HediffDef == hediffDef);
	}
}

internal readonly struct PendingApplication
{
	public readonly Pawn Pawn;

	public readonly HediffDef HediffDef;

	public PendingApplication(Pawn pawn, HediffDef hediffDef)
	{
		Pawn = pawn;
		HediffDef = hediffDef;
	}
}

public class GameComponent_EquipHediffApplicationQueue : GameComponent
{
	public GameComponent_EquipHediffApplicationQueue(Game game)
	{
	}

	public override void GameComponentTick()
	{
		base.GameComponentTick();
		if (Find.TickManager.TicksGame % 60 == 0)
		{
			PendingEquipHediffApplications.Tick();
		}
	}
}
