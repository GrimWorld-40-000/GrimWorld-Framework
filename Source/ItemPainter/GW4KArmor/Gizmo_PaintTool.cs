using System.Collections.Generic;
using GW4KArmor.UI;
using UnityEngine;
using Verse;

namespace GW4KArmor
{
    public class Gizmo_PaintableMulti : Command
    {
        public Pawn pawn;
        
        private static List<ThingWithComps> tempList = [];
        
        public Gizmo_PaintableMulti()
        {
            //icon = PaintContent.PaintIcon;
            icon = PaintContent.PaintIconMulti;
        }
        
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                if (pawn is { equipment: not null, apparel: not null })
                {
                    tempList.Clear();
                    tempList.AddRange(pawn.equipment.AllEquipmentListForReading);
                    tempList.AddRange(pawn.apparel.WornApparel);
                    
                    foreach (ThingWithComps thing in tempList)
                    {
                        Comp_TriColorMask comp = thing.GetComp<Comp_TriColorMask>();
                        
                        if (comp != null)
                        {
                            yield return new FloatMenuOption($"Paint {thing.LabelCap}", delegate ()
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
        private static (Color[] colors, int maskIndex) Clipboard { get; set; }
        
        public Comp_TriColorMask paintComp;
        
        private static ThingDef ClipBoardDef;

        public Gizmo_Paintable()
        {
            //icon = PaintContent.PaintIcon;
            icon = PaintContent.PaintIcon;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new (topLeft.x, topLeft.y, 75f, 75f);
            Rect top = rect.TopHalf().ContractedBy(2).RightPart(0.75f);
            Rect copyRect = top.LeftHalf().Rounded();
            Rect pasteRect = top.RightHalf().Rounded();

            bool button1Hovered = Mouse.IsOver(copyRect);
            bool button2Hovered = Mouse.IsOver(pasteRect);

            bool button1 = Widgets.ButtonInvisible(copyRect);
            bool button2 = Widgets.ButtonInvisible(pasteRect);
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            GUI.color = button1Hovered ? GenUI.MouseoverColor : Color.white;
            GUI.DrawTexture(copyRect, TexButton.Copy);
            GUI.color = Color.white;
            
            if (button1)
            {
                ClipBoardDef = paintComp.parent.def;
                Clipboard = (paintComp.Copy(), paintComp.MaskIndex);
                return new GizmoResult(GizmoState.Clear);
            }
            
            bool active = ClipBoardDef == paintComp.parent.def;
            GUI.color = active ? (button2Hovered ? GenUI.MouseoverColor : Color.white) : Color.gray;
            GUI.DrawTexture(pasteRect, TexButton.Paste);
            GUI.color = Color.white;
            
            if (!button2 || !active) 
                return result;
            
            paintComp.Paste(Clipboard.colors);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}