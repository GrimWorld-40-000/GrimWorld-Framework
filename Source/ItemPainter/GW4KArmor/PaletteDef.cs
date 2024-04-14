using System.Collections.Generic;
using GW4KArmor.Data;
using JetBrains.Annotations;
using Verse;

namespace GW4KArmor
{
    [UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
    public class PaletteDef : Def
    {
        public List<Palette> palettes = new List<Palette>();

        public override void PostLoad()
        {
            base.PostLoad();
            foreach (Palette palette in this.palettes)
            {
                palette.canBeDeleted = false;
                bool flag = !string.IsNullOrWhiteSpace(palette.name);
                if (!flag)
                {
                    Core.Error("Missing <name> from a palette in def '" + this.defName + "'", null);
                    palette.name = "---";
                }
            }
        }
    }
}