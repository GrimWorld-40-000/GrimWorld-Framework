using Verse;

namespace GW_Frame.Settings
{
    // Placeholder record required by the tab def system.
    // Module-specific settings are registered separately via SettingsTabRecord_Main.RegisterSection.
    public class SettingsRecord_Main : SettingsRecord
    {
        public override void CastChanges() { }
        public override void Reset() { }
        public override void ExposeData() { }
    }
}
