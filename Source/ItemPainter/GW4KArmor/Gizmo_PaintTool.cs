using System;
using System.Collections.Generic;
using GW4KArmor.UI;
using UnityEngine;
using Verse;

namespace GW4KArmor;

public class Gizmo_PaintableMulti : Command
{
    public Pawn pawn;
    
    public Gizmo_PaintableMulti()
    {
        //icon = PaintContent.PaintIcon;
        icon = PaintContent.PaintIconMulti;
    }

    private static List<ThingWithComps> tempList = new();
    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    {
        get
        {
            if (pawn == null) yield break;
            if (pawn.equipment != null && pawn.apparel != null)
            {
                tempList.Clear();
                tempList.AddRange(pawn.equipment.AllEquipmentListForReading);
                tempList.AddRange(pawn.apparel.WornApparel);
                foreach (var thing in tempList)
                {
                    var comp = thing.GetComp<Comp_TriColorMask>();
                    if (comp != null)
                    {
                        yield return new FloatMenuOption($"Paint {thing.LabelCap}", delegate()
                        {
                            Window_ItemPainter.OpenWindowFor(comp.parent);
                        });
                    }
                }
            }
        }
    }
}

public class Gizmo_Paintable : Command_Action
{
    private static ThingDef ClipBoardDef;
    private static (Color[] colors, int maskIndex) Clipboard { get; set; }
    
    public Comp_TriColorMask paintComp;
    
    public Gizmo_Paintable()
    {
        //icon = PaintContent.PaintIcon;
        icon = PaintContent.PaintIcon;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var rect = new Rect(topLeft.x, topLeft.y, 75f, 75f);
        var top = rect.TopHalf().ContractedBy(2).RightPart(0.75f);
        var copyRect = top.LeftHalf().Rounded();
        var pasteRect = top.RightHalf().Rounded();

        var button1Hovered = Mouse.IsOver(copyRect);
        var button2Hovered = Mouse.IsOver(pasteRect);
        
        var button1 = Widgets.ButtonInvisible(copyRect);
        var button2 = Widgets.ButtonInvisible(pasteRect);
        var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
        
        GUI.color = button1Hovered ? GenUI.MouseoverColor : Color.white;
        GUI.DrawTexture(copyRect, TexButton.Copy);
        GUI.color = Color.white;
        if (button1)
        {
            ClipBoardDef = paintComp.parent.def;
            Clipboard = (paintComp.Copy(), paintComp.MaskIndex);
            return new GizmoResult(GizmoState.Clear);
        }

        var active = ClipBoardDef == paintComp.parent.def;
        GUI.color = active ? (button2Hovered ? GenUI.MouseoverColor : Color.white) : Color.gray;
        GUI.DrawTexture(pasteRect, TexButton.Paste);
        GUI.color = Color.white;
        if (button2 && active)
        {
            paintComp.Paste(Clipboard.colors);
            return new GizmoResult(GizmoState.Clear);
        }

        return result;
    }
}