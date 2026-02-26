using System.Collections.Generic;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Utilities;
using HarmonyLib;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.ProgressionEducation
{
    // ProgressionEducation updates pawn timetables directly, while Better Pawn Control persists schedules per policy link.
    // Updating class hours in BPC's policy avoids schedule drift when BPC reapplies policy data.
    [HarmonyPatch(typeof(TimeAssignmentUtility), "SetPawnSchedules")]
    public static class TimeAssignmentUtility_SetPawnSchedules_Patch
    {
        public static bool Prefix(StudyGroup studyGroup, List<Pawn> participants, TimeAssignmentDef assignment = null)
        {
            var policyId = ScheduleUtility.defaultClassPolicyId;
            var activePolicyId = ScheduleManagerWrapper.GetActivePolicyIdOrDefault();
            var targetMap = studyGroup?.Map;
            var targetMapId = targetMap.uniqueID;

            foreach (var participant in participants)
            {
                var scheduleLink = ScheduleManagerWrapper.GetOrCreateScheduleLink(participant, policyId, targetMapId);
                ScheduleUtility.SetBPCAssignment(scheduleLink, studyGroup, assignment);
            }

            // set to current schedule if targeted policy is the current schedule
            return policyId == activePolicyId;
        }
    }
}

