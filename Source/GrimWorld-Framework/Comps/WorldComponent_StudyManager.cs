using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GW_Frame
{
    public class WorldComponent_StudyManager : WorldComponent
    {
        public WorldComponent_StudyManager(World world) : base(world)
        {
            ResearchProjects = DefDatabase<ResearchProjectDef>.AllDefs.Where(p => p.GetModExtension<DefModExtension_ExtraPrerequisiteActions>() != null);
        }

        public IEnumerable<ResearchProjectDef> ResearchProjects { get; }

        public void AddCompletedRequirement(ResearchProjectDef project, ThingDef itemCompleted)
        {
            if (!_extraRequisitesTracker.ContainsKey(project))
                _extraRequisitesTracker.Add(project, new ScribeDictionary<ThingDef, bool>(LookMode.Def, LookMode.Value));

            var projectRequisites = _extraRequisitesTracker[project];
            if (!projectRequisites.ContainsKey(itemCompleted))
            {
                projectRequisites.Add(itemCompleted, true);
                return;
            }

            projectRequisites[itemCompleted] = true;
        }

        public void CompleteAllRequirements(ResearchProjectDef project)
        {
            var extraReqExtension = project.GetModExtension<DefModExtension_ExtraPrerequisiteActions>();

            foreach (var req in extraReqExtension.ItemStudyRequirements)
            {
                AddCompletedRequirement(project, req.StudyObject);
            }
        }

        public bool CompletedAllRequirements(ResearchProjectDef project)
        {
            var extraReqExtension = project.GetModExtension<DefModExtension_ExtraPrerequisiteActions>();
            if (!_extraRequisitesTracker.ContainsKey(project))
                return false;
            var tracked = _extraRequisitesTracker[project];
            var completed = true;
            foreach (var req in extraReqExtension.ItemStudyRequirements)
            {
                if (tracked.TryGetValue(req.StudyObject, out bool foundResult))
                    completed &= foundResult;
                else return false;
            }
            return completed;
        }

        public bool CompletedRequirement(ResearchProjectDef project, ThingDef itemCompleted)
        {
            if (_extraRequisitesTracker.TryGetValue(project, out ScribeDictionary<ThingDef, bool> projectTracker))
                if (projectTracker.TryGetValue(itemCompleted, out bool foundResult))
                    return foundResult;
            return false;
        }

        public Dictionary<ResearchProjectDef, ScribeDictionary<ThingDef, bool>> _extraRequisitesTracker = new Dictionary<ResearchProjectDef, ScribeDictionary<ThingDef, bool>>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _extraRequisitesTracker, "ExtraRequisitesTracker", LookMode.Def, LookMode.Deep);
            base.ExposeData();
        }
    }
}