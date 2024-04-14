using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GW4KArmor
{
    public class Command_ActionWithPaintOptions : Command_Action
    {
        public Command_ActionWithPaintOptions()
        {
        }

        public static (Color[] colors, int maskIndex)? Clipboard { get; set; }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => this.GenerateOptions();
        public Comp_TriColorMask Comp { get; set; }

        private IEnumerable<FloatMenuOption> GenerateOptions()
        {
            yield return new FloatMenuOption("Copy",
                delegate
                {
                    Clipboard = new ValueTuple<Color[], int>(Comp.Copy(), Comp.MaskIndex);
                });
            yield return new FloatMenuOption("Paste", delegate
            {
                var flag = Clipboard == null;
                if (!flag)
                {
                    Comp.MaskIndex = Mathf.Clamp(Clipboard.Value.maskIndex, 0,
                        Comp.Props.maskCount - 1);
                    Comp.Paste(Clipboard.Value.colors);
                    Comp.MarkDirty();
                }
            })
            {
                Disabled = Clipboard == null
            };
        }
    }
}