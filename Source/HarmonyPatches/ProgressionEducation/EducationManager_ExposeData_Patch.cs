using HarmonyLib;
using ProgressionEducation;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.ProgressionEducation
{
    [HarmonyPatch(typeof(EducationManager), nameof(EducationManager.ExposeData))]
    public static class EducationManager_ExposeData_Patch
    {
        public static void Postfix(EducationManager __instance)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                return;
            }

            TimeAssignmentUtility.RemoveAllDynamicTimeAssignmentDefs();
            foreach (var studyGroup in __instance.studyGroups)
            {
                EducationLog.Message($"Generating TimeAssignmentDef for study group '{studyGroup.className}'");
                TimeAssignmentUtility.GenerateTimeAssignmentDef(studyGroup);
            }
        }
    }
}
