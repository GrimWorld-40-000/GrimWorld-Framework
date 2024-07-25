using GW_Frame.Settings;
using Verse;

namespace Chamber.Settings
{
    public class SettingsRecord_Chamber: SettingsRecord
    {
        public static float brainDamageChance;
        public static int daysToFinish;
        
        
        public override void CastChanges() { }

        public override void Reset()
        {
            daysToFinish = 10;
            brainDamageChance = 0.6f;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<float>(ref brainDamageChance, "brainDamageChance",0.6f,true);
            Scribe_Values.Look<int>(ref daysToFinish, "daysToFinish", forceSave: true);
        }
    }
}