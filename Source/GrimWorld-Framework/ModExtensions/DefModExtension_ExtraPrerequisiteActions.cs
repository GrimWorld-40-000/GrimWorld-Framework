using System.Collections.Generic;
using Verse;

namespace GW_Frame
{
    /// <summary>
    /// DefModExtension's are not meant to save fields, only to hold static data.
    /// Use a Comp if you need to change/save/persist data.
    /// </summary>
    public class DefModExtension_ExtraPrerequisiteActions : DefModExtension, IExposable
    {
        public ThingDef StudyLocation;
        public List<StudyRequirement> ItemStudyRequirements;
        public bool longLabel = false; // Set this to true if a two line label is causing display issues

        public void ExposeData()
        {
            Scribe_Defs.Look(ref StudyLocation, "StudyLocation");
            Scribe_Collections.Look(ref ItemStudyRequirements, "StudyRequirements", LookMode.Deep);
        }
    }
}