using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GW_Frame.Settings
{
    public class SettingsRecord_Patches : SettingsRecord
    {
        public static readonly string[] GW_KEYS =
        {
            "GWCat", "GWResearch",
            "GWBalance", "GWGene", "VEHook" // Gravship and Core integrated, Aspectus empty
        };

        private Dictionary<string, bool> patches;

        /// <summary>Default for new installs, missing keys after mod update, and Reset in mod settings.</summary>
        public static bool DefaultEnabledForKey(string key) =>
            key != "GWCat" && key != "GWBalance" && key != "GWGene";

        public bool Get(string key) => patches != null && patches.TryGetValue(key, out var v) && v;

        public void Set(string key, bool value)
        {
            if (patches == null) patches = new Dictionary<string, bool>();
            patches[key] = value;
        }

        public void SetAll(bool value)
        {
            foreach (var key in GW_KEYS)
                Set(key, value);
        }

        public bool AnyEnabled => GW_KEYS.Any(k => Get(k));

        public override void Reset()
        {
            patches = new Dictionary<string, bool>();
            foreach (var key in GW_KEYS)
                patches[key] = DefaultEnabledForKey(key);
        }

        public override void CastChanges() { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref patches, "gwPatches", LookMode.Value, LookMode.Value);
            if (patches == null)
                patches = new Dictionary<string, bool>();
            foreach (var key in GW_KEYS)
            {
                if (!patches.ContainsKey(key))
                    patches[key] = DefaultEnabledForKey(key);
            }
        }
    }
}
