using System.Collections.Generic;
using GW4KArmor.Data;
using GW4KArmor.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor;

public class Comp_TriColorMask : ThingComp
{
	public int MaskIndex;
	private Color? colorOne;
	private Color? colorTwo;
	private Color? colorThree;
	private CompColorable colorComp;

	public CompProperties_TriColorMask Props => props as CompProperties_TriColorMask;

	public MaskTextureStorage Masks => Props.Masks;

	public ThingDef Def => parent.def;

	public Color ColorOne
	{
		get
		{
			var compColorable = colorComp;
			return compColorable is { Active: true } ? colorComp.Color : colorOne ?? Color.white;
		}
		set
		{
			var flag = colorComp == null;
			if (flag)
				colorOne = value;
			else
				colorComp.SetColor(value);
		}
	}

	public Color ColorTwo
	{
		get => colorTwo ?? Color.white;
		set => colorTwo = value;
	}

	public Color ColorThree
	{
		get => colorThree ?? Color.white;
		set => colorThree = value;
	}

	public Color[] Copy()
	{
		return new[]
		{
			ColorOne,
			ColorTwo,
			ColorThree
		};
	}

	public void Paste(Color[] colors)
	{
		ColorOne = colors[0];
		ColorTwo = colors[1];
		ColorThree = colors[2];
	}

	public void MarkDirty()
	{
		parent?.Notify_ColorChanged();
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		colorComp = parent.GetComp<CompColorable>();
		
		if (respawningAfterLoad) return;
		if (Props.defaultPalette != null && colorOne == null && colorTwo == null && colorThree == null)
		{
			colorOne = Props.defaultPalette.colorA;
			colorTwo = Props.defaultPalette.colorB;
			colorThree = Props.defaultPalette.colorC;
			MarkDirty();
		}
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		if (pawn.kindDef.HasModExtension<DefaultPaletteExtension>())
		{
			var extension = pawn.kindDef.GetModExtension<DefaultPaletteExtension>();
			var palette = extension.defaultPalette;
			colorOne = palette.colorA;
			colorTwo = palette.colorB;
			colorThree = palette.colorC;
			MarkDirty();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref colorOne, "colorOne", Color.white);
		Scribe_Values.Look(ref colorTwo, "colorTwo", Color.white);
		Scribe_Values.Look(ref colorThree, "colorThree", Color.white);
		Scribe_Values.Look(ref MaskIndex, "maskIndex");
	}

	private Gizmo_Paintable _gizmo;
	private Gizmo_PaintableMulti _gizmoMulti;

	internal Gizmo_Paintable PaintGizmo
	{
		get
		{
			var isDisabled = parent is Apparel { Wearer: Pawn { IsColonistPlayerControlled: false } };
			var gizm = _gizmo ??= new Gizmo_Paintable
			{
				paintComp = this,
				defaultLabel = "Set Colors", 
				alsoClickIfOtherInGroupClicked = true,
				action = () => { Window_ItemPainter.OpenWindowFor(parent); }
			};

			gizm.defaultDesc = isDisabled
				? "Cannot change colors on other faction!"
				: "Change the 3 colors of this item.\nRight-click to copy-paste the color and mask to other objects.";
			gizm.disabled = isDisabled;
			return gizm;
		}
	}

	public Gizmo_PaintableMulti PaintGizmoMulti
	{
		get
		{
			var isDisabled = parent is Apparel { Wearer: Pawn { IsColonistPlayerControlled: false } };
			var gizm = _gizmoMulti ??= new Gizmo_PaintableMulti
			{
				pawn = ParentHolder as Pawn,
				defaultLabel = "Color Apparel",
				alsoClickIfOtherInGroupClicked = true,
			};
			
			gizm.defaultDesc = isDisabled
				? "Cannot change colors on other faction!"
				: "Change the 3 colors of an equipped colorable item!";
			gizm.disabled = isDisabled;
			return gizm;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (var gizmo in base.CompGetGizmosExtra())
		{
			yield return gizmo;
		}
		yield return PaintGizmo;
	}
}