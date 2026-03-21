using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.Verse
{
    internal static class ModMetadataCompatibility
    {
        internal const string BetterPawnControlPackageId = "VouLT.BetterPawnControl";
        internal const string ProgressionEducationPackageId = "ferny.ProgressionEducation";

        internal static bool IsProgressionEducation(ModMetaData modMetaData)
        {
            return modMetaData != null &&
                modMetaData.SamePackageId(ProgressionEducationPackageId, ignorePostfix: true);
        }
    }

    [HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.IncompatibleWith), MethodType.Getter)]
    internal static class ModMetaData_IncompatibleWith_Patch
    {
        public static void Postfix(ModMetaData __instance, ref List<string> __result)
        {
            if (!ModMetadataCompatibility.IsProgressionEducation(__instance) || __result == null)
            {
                return;
            }

            __result = __result
                .Where(packageId => !string.Equals(
                    packageId,
                    ModMetadataCompatibility.BetterPawnControlPackageId,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    [HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.GetRequirements))]
    internal static class ModMetaData_GetRequirements_Patch
    {
        public static void Postfix(ModMetaData __instance, ref IEnumerable<ModRequirement> __result)
        {
            if (!ModMetadataCompatibility.IsProgressionEducation(__instance) || __result == null)
            {
                return;
            }

            __result = __result.Where(requirement =>
                requirement is not ModIncompatibility incompatibility ||
                !string.Equals(
                    incompatibility.packageId,
                    ModMetadataCompatibility.BetterPawnControlPackageId,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
