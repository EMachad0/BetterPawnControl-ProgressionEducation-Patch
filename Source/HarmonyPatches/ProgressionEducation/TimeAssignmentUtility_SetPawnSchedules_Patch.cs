using System.Collections.Generic;
using BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl;
using HarmonyLib;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch
{
    [HarmonyPatch(typeof(TimeAssignmentUtility), "SetPawnSchedules")]
    public static class TimeAssignmentUtility_SetPawnSchedules_Patch
    {
        public static bool Prefix(StudyGroup studyGroup, List<Pawn> participants, TimeAssignmentDef assignment = null)
        {
            var policyId = 0;
            var activePolicyId = ScheduleManagerWrapper.GetActivePolicyIdOrDefault();

            foreach (var participant in participants)
            {
                var scheduleLink = getDefaultScheduleLink(participant, policyId);
                if (scheduleLink == null)
                {
                    Log.Error($"[BPC PE Patch] ScheduleLink 0 not found.");
                    continue;
                }

                for (int hour = 0; hour < 24; hour++)
                {
                    bool isScheduled;
                    if (studyGroup.startHour <= studyGroup.endHour)
                    {
                        isScheduled = hour >= studyGroup.startHour && hour <= studyGroup.endHour;
                    }
                    else
                    {
                        isScheduled = hour >= studyGroup.startHour || hour <= studyGroup.endHour;
                    }

                    if (isScheduled)
                    {
                        TimeAssignmentDef assignmentToSet = assignment ?? ((hour > 5 && hour <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep);
                        SetBPCAssignment(hour, assignmentToSet, scheduleLink);
                        if (policyId == activePolicyId)
                        {
                            participant.timetable.SetAssignment(hour, assignmentToSet);
                        }
                    }
                }
            }

            return false;
        }

        private static void SetBPCAssignment(int hour, TimeAssignmentDef assignment, ScheduleLinkWrapper link)
        {
            if (!link.TrySetAssignment(hour, assignment))
            {
                Log.Error($"[BPC PE Patch] Invalid schedule index {hour} for pawn {link.colonist.LabelShort}.");
                return;
            }

            EducationLog.Message($"Set timetable for pawn {link.colonist.LabelShort} on BPC policy {link.zone} at hour {hour} to {assignment.defName}");
        }

        private static ScheduleLinkWrapper getDefaultScheduleLink(Pawn pawn, int policyId)
        {
            return ScheduleManagerWrapper
                    .EnumerateScheduleLinks()
                    .FirstOrFallback(l => l.zone == policyId && l.colonist != null && pawn.Equals(l.colonist) && l.mapId == pawn.Map.uniqueID);
        }
    }
}

