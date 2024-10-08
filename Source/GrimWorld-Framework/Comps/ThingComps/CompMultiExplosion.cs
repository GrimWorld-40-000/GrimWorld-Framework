using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GW_Frame.Comps.ThingComps
{
	public class CompProperties_MultiExplosive : CompProperties
	{
		public List<float> explosiveRadii;
		public FloatRange timeBetweenExplosions;
		public bool onlyExplodeWhenOn;
		public bool scaleByFuelPercentage;

		public DamageDef explosiveDamageType;
		public int damageAmountBase = -1;
		public float armorPenetrationBase = -1f;
		public ThingDef postExplosionSpawnThingDef;
		public float postExplosionSpawnChance;
		public int postExplosionSpawnThingCount = 1;
		public bool applyDamageToExplosionCellsNeighbors;
		public ThingDef preExplosionSpawnThingDef;
		public float preExplosionSpawnChance;
		public int preExplosionSpawnThingCount = 1;
		public float chanceToStartFire;
		public bool damageFalloff;
		public bool explodeOnKilled;
		public bool explodeOnDestroyed;
		public GasType? postExplosionGasType;
		public bool doVisualEffects = true;
		public bool doSoundEffects = true;
		public float propagationSpeed = 1f;
		public float explosiveExpandPerStackcount;
		public float explosiveExpandPerFuel;
		public EffecterDef explosionEffect;
		public SoundDef explosionSound;
		public List<DamageDef> startWickOnDamageTaken;
		public float startWickHitPointsPercent = 0.2f;
		public IntRange wickTicks = new(140, 150);
		public float wickScale = 1f;
		public List<DamageDef> startWickOnInternalDamageTaken;
		public bool drawWick = true;
		public float chanceNeverExplodeFromDamage;
		public float destroyThingOnExplosionSize;
		public DamageDef requiredDamageTypeToExplode;
		public IntRange? countdownTicks;
		public string extraInspectStringKey;
		public List<WickMessage> wickMessages;

		public CompProperties_MultiExplosive()
		{
			compClass = typeof(CompMultiExplosive);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			if (explosiveDamageType != null)
				return;
			explosiveDamageType = DamageDefOf.Bomb;
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (var configError in base.ConfigErrors(parentDef))
				yield return configError;
			if (parentDef.tickerType != TickerType.Normal)
				yield return "CompMultiExplosive requires Normal ticker type";
		}
	}

	public class CompMultiExplosive : ThingComp
	{
		public bool wickStarted;
		public int wickTicksLeft;
		private Thing instigator;
		private int countdownTicksLeft = -1;
		public bool destroyedThroughDetonation;
		private List<Thing> thingsIgnoredByExplosion;
		public float? customExplosiveRadius;
		protected Sustainer wickSoundSustainer;
		private OverlayHandle? overlayBurningWick;

		private int explosionIndex;
		private int minorExplosionTicksLeft;

		public CompProperties_MultiExplosive Props => (CompProperties_MultiExplosive)props;

		protected int StartWickThreshold => Mathf.RoundToInt(Props.startWickHitPointsPercent * parent.MaxHitPoints);

		private float Scaling => Props.scaleByFuelPercentage ? FuelComp.FuelPercentOfMax : 1;
		private CompRefuelable FuelComp => _fuelComp ??= parent.GetComp<CompRefuelable>();
		private CompRefuelable _fuelComp;
		private CompPowerTrader PowerComp => _powerComp ??= parent.GetComp<CompPowerTrader>();
		private CompPowerTrader _powerComp;

		private float ExplosiveRadius => Props.explosiveRadii[explosionIndex] * Scaling;
		private int ExplosiveDamage => (int)(Props.damageAmountBase * Scaling);

		protected virtual bool CanEverExplodeFromDamage
		{
			get
			{
				if (Props.chanceNeverExplodeFromDamage < 9.999999747378752E-06)
					return true;
				Rand.PushState();
				Rand.Seed = parent.thingIDNumber.GetHashCode();
				var num = Rand.Value > (double)Props.chanceNeverExplodeFromDamage ? 1 : 0;
				Rand.PopState();
				return num != 0;
			}
		}

		public void AddThingsIgnoredByExplosion(List<Thing> things)
		{
			thingsIgnoredByExplosion ??= new List<Thing>();
			thingsIgnoredByExplosion.AddRange(things);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref instigator, "instigator");
			Scribe_Collections.Look(ref thingsIgnoredByExplosion, "thingsIgnoredByExplosion",
				LookMode.Reference);
			Scribe_Values.Look(ref wickStarted, "wickStarted");
			Scribe_Values.Look(ref wickTicksLeft, "wickTicksLeft");
			Scribe_Values.Look(ref destroyedThroughDetonation, "destroyedThroughDetonation");
			Scribe_Values.Look(ref countdownTicksLeft, "countdownTicksLeft");
			Scribe_Values.Look(ref customExplosiveRadius, "explosiveRadius");
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (Props.countdownTicks.HasValue)
				countdownTicksLeft = Props.countdownTicks.Value.RandomInRange;
			UpdateOverlays();
		}

		public override void CompTick()
		{
			if (countdownTicksLeft > 0)
			{
				--countdownTicksLeft;
				if (countdownTicksLeft == 0)
				{
					StartWick();
					countdownTicksLeft = -1;
				}
			}

			if (!wickStarted)
				return;

			//Cannot break while exploding.
			parent.HitPoints = (int)(parent.MaxHitPoints * Props.startWickHitPointsPercent);

			if (wickSoundSustainer == null)
				StartWickSustainer();
			else
				wickSoundSustainer.Maintain();
			if (Props.wickMessages != null)
				foreach (var wickMessage in Props.wickMessages.Where(wickMessage =>
					         wickMessage.ticksLeft == wickTicksLeft && wickMessage.wickMessagekey != null))
					Messages.Message(
						wickMessage.wickMessagekey.Translate(
							(NamedArgument)parent.GetCustomLabelNoCount(false),
							(NamedArgument)wickTicksLeft.ToStringSecondsFromTicks()),
						(Thing)parent, wickMessage.messageType ?? MessageTypeDefOf.NeutralEvent,
						false);

			--wickTicksLeft;
			if (wickTicksLeft > 0)
				return;

			--minorExplosionTicksLeft;
			if (minorExplosionTicksLeft > 0) return;
			minorExplosionTicksLeft = (int)(Props.timeBetweenExplosions.RandomInRange * 60);

			if (explosionIndex >= Props.explosiveRadii.Count - 1)
			{
				Detonate(parent.MapHeld);
			}
			else
			{
				DoMinorExplosion(parent.MapHeld);
				DoMinorExplosion(parent.MapHeld);
				explosionIndex++;
			}
		}

		private void StartWickSustainer()
		{
			SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(parent.PositionHeld,
				parent.MapHeld));
			var info = SoundInfo.InMap((TargetInfo)(Thing)parent, MaintenanceType.PerTick);
			wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
		}

		private void EndWickSustainer()
		{
			if (wickSoundSustainer == null)
				return;
			wickSoundSustainer.End();
			wickSoundSustainer = null;
		}

		private void UpdateOverlays()
		{
			if (!parent.Spawned || !Props.drawWick)
				return;
			parent.Map.overlayDrawer.Disable(parent, ref overlayBurningWick);
			if (!wickStarted)
				return;
			overlayBurningWick =
				parent.Map.overlayDrawer.Enable(parent, OverlayTypes.BurningWick);
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			if ((mode != DestroyMode.KillFinalize || !Props.explodeOnKilled) && !Props.explodeOnDestroyed)
				return;
			Detonate(previousMap, true);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			StopWick();
		}

		public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			if (!CanEverExplodeFromDamage)
				return;
			if (wickStarted || Props.startWickOnDamageTaken == null ||
			    !Props.startWickOnDamageTaken.Contains(dinfo.Def))
				return;
			StartWick(dinfo.Instigator);
		}

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (!CanEverExplodeFromDamage || !CanExplodeFromDamageType(dinfo.Def) || parent.Destroyed)
				return;
			if (wickStarted && dinfo.Def == DamageDefOf.Stun)
			{
				StopWick();
			}
			else
			{
				if (wickStarted || parent.HitPoints > StartWickThreshold ||
				    (!dinfo.Def.ExternalViolenceFor(parent) &&
				     (Props.startWickOnInternalDamageTaken.NullOrEmpty() ||
				      !Props.startWickOnInternalDamageTaken.Contains(dinfo.Def))))
					return;
				StartWick(dinfo.Instigator);
			}
		}

		public void StartWick()
		{
			StartWick(null);
		}

		public void StartWick(Thing instigatorKnown)
		{
			if (Props.onlyExplodeWhenOn && !PowerComp.PowerOn) return;
			if (wickStarted || ExplosiveRadius <= 0.0)
				return;
			instigator = instigatorKnown;
			wickStarted = true;
			wickTicksLeft = Props.wickTicks.RandomInRange;
			StartWickSustainer();
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(parent, Props.explosiveDamageType,
				instigator: instigatorKnown);
			UpdateOverlays();
		}

		public void StopWick()
		{
			wickStarted = false;
			instigator = null;
			UpdateOverlays();
			EndWickSustainer();
		}

		protected void DoMinorExplosion(Map map, bool forceCenter = false, bool ignoreUnspawned = false)
		{
			var propsMultiExplosive = Props;
			if (ignoreUnspawned && !parent.SpawnedOrAnyParentSpawned) return;
			var explosiveRadius = ExplosiveRadius;
			if (explosiveRadius <= 0.0)
				return;
			var knownInstigatorThing =
				instigator == null || (instigator.HostileTo(parent.Faction) &&
				                       parent.Faction != Faction.OfPlayer)
					? parent
					: instigator;

			if (map == null)
			{
				Log.Warning("Tried to detonate CompExplosive in a null map.");
			}
			else
			{
				var cell = forceCenter
					? parent.PositionHeld
					: parent.OccupiedRect().GetCellsOnEdge(Rot4.Random).RandomElement();

				if (propsMultiExplosive.explosionEffect != null)
				{
					var effecter = propsMultiExplosive.explosionEffect.Spawn();
					effecter.Trigger(new TargetInfo(cell, map),
						new TargetInfo(cell, map));
					effecter.Cleanup();
				}

				//Make sure parent is undamaged by explosion
				(thingsIgnoredByExplosion ??= new List<Thing>()).AddDistinct(parent);

				GenExplosion.DoExplosion(cell, map, explosiveRadius, propsMultiExplosive.explosiveDamageType,
					knownInstigatorThing, ExplosiveDamage, propsMultiExplosive.armorPenetrationBase, propsMultiExplosive.explosionSound,
					null, null, null, propsMultiExplosive.postExplosionSpawnThingDef, propsMultiExplosive.postExplosionSpawnChance,
					propsMultiExplosive.postExplosionSpawnThingCount, Props.postExplosionGasType,
					propsMultiExplosive.applyDamageToExplosionCellsNeighbors, propsMultiExplosive.preExplosionSpawnThingDef,
					propsMultiExplosive.preExplosionSpawnChance, propsMultiExplosive.preExplosionSpawnThingCount, propsMultiExplosive.chanceToStartFire,
					propsMultiExplosive.damageFalloff, new float?(), thingsIgnoredByExplosion, new FloatRange?(),
					propsMultiExplosive.doVisualEffects, propsMultiExplosive.propagationSpeed, 0, propsMultiExplosive.doSoundEffects);
			}
		}

		protected void Detonate(Map map, bool ignoreUnspawned = false)
		{
			if (parent.Destroyed) return;
			destroyedThroughDetonation = true;
			parent.Kill();

			DoMinorExplosion(map, true, ignoreUnspawned);
		}

		private bool CanExplodeFromDamageType(DamageDef damage)
		{
			return Props.requiredDamageTypeToExplode == null || Props.requiredDamageTypeToExplode == damage;
		}

		public override string CompInspectStringExtra()
		{
			var str = "";
			if (countdownTicksLeft != -1)
				str = str +
				      "DetonationCountdown".Translate(
					      (NamedArgument)countdownTicksLeft.TicksToDays().ToString("0.0"));
			if (Props.extraInspectStringKey != null)
				str = str + ((str != "" ? "\n" : "") + Props.extraInspectStringKey.Translate());
			return str;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (countdownTicksLeft > 0)
			{
				var commandAction = new Command_Action();
				commandAction.defaultLabel = "DEV: Trigger countdown";
				commandAction.action = StartWick;
				yield return commandAction;
			}
		}
	}
}