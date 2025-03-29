using Verse;

namespace GW_Frame.Settings
{
    public abstract class SettingsRecord : IExposable
    {
        public abstract void CastChanges();
        public abstract void Reset();
        public abstract void ExposeData();
    }
}
