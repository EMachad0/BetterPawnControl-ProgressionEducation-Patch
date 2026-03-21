using HarmonyLib;
using ProgressionEducation;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.ProgressionEducation
{
    [HarmonyPatch(typeof(EducationManager), nameof(EducationManager.FinalizeInit))]
    public static class EducationManager_FinalizeInit_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
