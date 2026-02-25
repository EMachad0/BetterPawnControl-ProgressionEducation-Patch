using System.Collections.Generic;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using BetterPawnControlProgressionEducationPatch.Utilities;
using HarmonyLib;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.HarmonyPatches.ProgressionEducation
{
    [HarmonyPatch(typeof(TimeAssignmentUtility), "SetPawnSchedules")]
    public static class TimeAssignmentUtility_SetPawnSchedules_Patch
    {
        public static bool Prefix(StudyGroup studyGroup, List<Pawn> participants, TimeAssignmentDef assignment = null)
        {
            var policyId = ScheduleUtility.defaultClassPolicyId;
            var activePolicyId = ScheduleManagerWrapper.GetActivePolicyIdOrDefault();

            foreach (var participant in participants)
            {
                var scheduleLink = ScheduleUtility.GetScheduleLink(participant, policyId);
                if (scheduleLink == null)
                {
                    Log.Error($"[BPC PE Patch] ScheduleLink for {participant.LabelShort} and policy {policyId} not found.");
                    continue;
                }

                ScheduleUtility.SetBPCAssignment(scheduleLink, studyGroup, assignment);
            }

            // set to current schedule if targeted policy is the current schedule
            return policyId == activePolicyId;
        }
    }
}

