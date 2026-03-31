using Chamber.Settings;
using Verse;
using GWSettings = GW_Frame.Settings;

namespace Chamber
{
    [StaticConstructorOnStartup]
    public static class ChamberStartup
    {
        static ChamberStartup()
        {
            GWSettings.Settings.Instance.TryGetModSettings<SettingsRecord_Chamber>(out _);

            GWSettings.SettingsTabRecord_Main.RegisterSection("Indoctrination Chamber", listing =>
            {
                float brainDamageChance = SettingsRecord_Chamber.brainDamageChance;
                int daysToFinish = SettingsRecord_Chamber.daysToFinish;

                listing.Label(string.Format("Brain damage chance (default 60%): {0:0}%", brainDamageChance * 100), -1f);
                SettingsRecord_Chamber.brainDamageChance = listing.Slider(brainDamageChance, 0.01f, 1.0f);
                listing.Gap();
                listing.Label(string.Format("Days to finish conversion (default 10): {0}", daysToFinish), -1f);
                SettingsRecord_Chamber.daysToFinish = (int)listing.Slider(daysToFinish, 1, 60);
            });
        }
    }
}
