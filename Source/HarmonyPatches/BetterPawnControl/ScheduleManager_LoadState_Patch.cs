using System;
using System.Collections.Generic;
using System.Reflection;
using BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Utilities;
using HarmonyLib;
using ProgressionEducation;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.BetterPawnControl
{
    // Loading a non-class BPC policy can leave dynamic class assignments in pawn timetables from prior class scheduling.
    // Normalizing those stale slots prevents education schedule defs from leaking into policies.
    [HarmonyPatch]
    public static class ScheduleManager_LoadState_Patch
    {
        // ScheduleManager is internal in BPC
        private static readonly Type ScheduleManagerType = AccessTools.TypeByName("BetterPawnControl.ScheduleManager");
        private static readonly Type ScheduleLinkType = AccessTools.TypeByName("BetterPawnControl.ScheduleLink");

        public static MethodBase TargetMethod()
        {
            if (ScheduleManagerType == null || ScheduleLinkType == null)
            {
                throw new MissingMemberException("Could not resolve BetterPawnControl.ScheduleManager or BetterPawnControl.ScheduleLink.");
            }

            var scheduleLinkListType = typeof(List<>).MakeGenericType(ScheduleLinkType);
            var method = AccessTools.Method(
                ScheduleManagerType,
                "LoadState",
                new[] { scheduleLinkListType, typeof(List<Pawn>), typeof(Policy) });

            return method ?? throw new MissingMethodException("Could not resolve ScheduleManager.LoadState(List<ScheduleLink>, List<Pawn>, Policy).");
        }

        public static void Postfix(
            [HarmonyArgument(1)] List<Pawn> pawns,
            [HarmonyArgument(2)] Policy policy
        )
        {
            var policyId = ScheduleManagerWrapper.GetPolicyIdOrDefault(policy);
            if (policyId == ScheduleUtility.defaultClassPolicyId)
            {
                return;
            }
            if (pawns == null)
            {
                return;
            }

            foreach (var pawn in pawns)
            {
                if (pawn?.timetable == null)
                {
                    continue;
                }

                for (int hour = 0; hour < 24; hour++)
                {
                    var timeDef = pawn.timetable.GetAssignment(hour);
                    if (TimeAssignmentUtility.IsStudyGroupAssignment(timeDef))
                    {
                        var assignmentToSet = ScheduleUtility.GetDefaultTimeAssignmentDef(hour);
                        pawn.timetable.SetAssignment(hour, assignmentToSet);
                    }
                }
            }
        }
    }
}
