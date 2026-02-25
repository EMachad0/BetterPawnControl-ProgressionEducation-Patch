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
                var scheduleLink = getScheduleLink(participant, policyId);
                if (scheduleLink == null)
                {
                    Log.Error($"[BPC PE Patch] ScheduleLink for {participant.LabelShort} and policy {policyId} not found.");
                    continue;
                }

                SetBPCAssignment(scheduleLink, studyGroup, assignment);
            }

            // set to current schedule if targeted policy is the current schedule
            return policyId == activePolicyId;
        }

        private static TimeAssignmentDef GetDefaultTimeAssignmentDef(int hour)
        {
            return (hour > 5 && hour <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep;
        }

        private static void TryRepairScheduleLink(ScheduleLinkWrapper scheduleLink)
        {
            if (scheduleLink.schedule is null)
            {
                Log.Error($"[BPC PE Patch] ScheduleLink without schedule for {scheduleLink.colonist.LabelShort} and policyId {scheduleLink.zone}.");
            }
            while (scheduleLink.schedule.Count < 24)
            {
                int hour = scheduleLink.schedule.Count;
                Log.Message($"Timetable for {scheduleLink.colonist.LabelShort} is incomplete. Appending hour {hour}.");
                scheduleLink.schedule.Add(GetDefaultTimeAssignmentDef(hour));
            }
        }

        private static void SetBPCAssignment(ScheduleLinkWrapper link, StudyGroup studyGroup, TimeAssignmentDef assignment = null)
        {
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
                    TimeAssignmentDef assignmentToSet = assignment ?? GetDefaultTimeAssignmentDef(hour);
                    if (!link.TrySetAssignment(hour, assignmentToSet))
                    {
                        Log.Error($"[BPC PE Patch] Invalid schedule index {hour} for pawn {link.colonist.LabelShort}.");
                        return;
                    }

                    EducationLog.Message($"Set timetable for pawn {link.colonist.LabelShort} on BPC policy {link.zone} at hour {hour} to {assignment.defName}");
                }
            }

        }

        private static ScheduleLinkWrapper getScheduleLink(Pawn pawn, int policyId)
        {
            return ScheduleManagerWrapper
                    .EnumerateScheduleLinks()
                    .FirstOrFallback(l => l.zone == policyId && l.colonist != null && pawn.Equals(l.colonist) && l.mapId == pawn.Map.uniqueID);
        }
    }
}

