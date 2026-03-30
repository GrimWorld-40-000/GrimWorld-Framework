using System.Collections.Generic;
using System.Xml;
using Verse;
using GW_Frame.Settings;

namespace GW_Frame
{
    public class PatchOperation_GWPatch : PatchOperation
    {
        public string patchName;
        public List<PatchOperation> operations;

        private static bool AnyGWActive()
        {
            return ModsConfig.IsActive("Grimworld.Core") ||
                   ModsConfig.IsActive("Grimworld.AstraMilitarum") ||
                   ModsConfig.IsActive("grimworld.talonOfTheEmperor") ||
                   ModsConfig.IsActive("grimworld.HighestRulers");
        }

        protected override bool ApplyWorker(XmlDocument xml)
        {
            SettingsRecord_Patches s = SettingsTabRecord_Patches.SettingsRecord;
            if (s == null) return true;

            bool enabled = false;

            switch (patchName)
            {
                case "GWCat":
                    enabled = s.Get("GWCat") && AnyGWActive();
                    break;

                case "GWCore":
                    enabled = s.Get("GWCore") && ModsConfig.IsActive("Grimworld.Core");
                    break;

                case "GWResearch":
                    enabled = s.Get("GWResearch") && AnyGWActive();
                    break;

                case "GWGravship":
                    enabled = s.Get("GWGravship") && AnyGWActive();
                    break;

                case "GWBalance":
                    enabled = s.Get("GWBalance") && AnyGWActive();
                    break;

                case "GWGene":
                    enabled = s.Get("GWGene") && AnyGWActive();
                    break;

                case "VEHook":
                    enabled = s.Get("VEHook") && AnyGWActive();
                    break;

                default:
                    enabled = false;
                    break;
            }

            if (!enabled) return true;

            if (!operations.NullOrEmpty())
            {
                foreach (PatchOperation op in operations)
                {
                    if (op is PatchOperationFindMod findMod)
                    {
                        if (!findMod.Apply(xml)) continue;
                    }
                    else if (op is PatchOperationAdd ||
                             op is PatchOperationRemove ||
                             op is PatchOperationReplace)
                    {
                        op.Apply(xml);
                    }
                    else
                    {
                        if (!op.Apply(xml)) return false;
                    }
                }
            }

            return true;
        }
    }
}
