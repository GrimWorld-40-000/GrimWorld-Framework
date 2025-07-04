using System.Collections.Generic;
using GW4KArmor.Data;
using GW4KArmor.Debugging;
using JetBrains.Annotations;
using Verse;

namespace GW4KArmor
{
    [UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
    public class PaletteDef : Def
    {
        public List<Palette> palettes = new();
        
        public override void PostLoad()
        {
            base.PostLoad();
            foreach (Palette palette in palettes)
            {
                palette.canBeDeleted = false;
                bool flag = !string.IsNullOrWhiteSpace(palette.name);

                if (flag) 
                    continue;
                
                GW4KLog.Error("Missing <name> from a palette in def: " + defName);
                palette.name = "---";
            }
        }
    }
}