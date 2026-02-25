using System;
using System.Collections.Generic;
using System.Reflection;
using BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Utilities;
using HarmonyLib;
using ProgressionEducation;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.BetterPawnControl
{
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

        public static void Postfix([HarmonyArgument(1)] List<Pawn> pawns, Policy policy)
        {
            if (policy.id == 0)
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
