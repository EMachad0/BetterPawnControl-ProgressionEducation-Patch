using System.Collections.Generic;
using BetterPawnControl;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl
{
    internal sealed class ScheduleLinkWrapper
    {
        private static readonly AccessTools.FieldRef<Link, int> ZoneRef =
            AccessTools.FieldRefAccess<Link, int>("zone");

        private static readonly AccessTools.FieldRef<Link, int> MapIdRef =
            AccessTools.FieldRefAccess<Link, int>("mapId");

        private static readonly AccessTools.FieldRef<ScheduleLink, Pawn> ColonistRef =
            AccessTools.FieldRefAccess<ScheduleLink, Pawn>("colonist");

        private static readonly AccessTools.FieldRef<ScheduleLink, List<TimeAssignmentDef>> ScheduleRef =
            AccessTools.FieldRefAccess<ScheduleLink, List<TimeAssignmentDef>>("schedule");

        private readonly ScheduleLink _link;

        public int zone => ZoneRef(_link);
        public int mapId => MapIdRef(_link);
        public Pawn colonist => ColonistRef(_link);
        public List<TimeAssignmentDef> schedule => ScheduleRef(_link);

        public ScheduleLinkWrapper(ScheduleLink link)
        {
            _link = link;
        }

        public static bool TryCreate(object rawLink, out ScheduleLinkWrapper wrapper)
        {
            if (rawLink is ScheduleLink scheduleLink)
            {
                wrapper = new ScheduleLinkWrapper(scheduleLink);
                return true;
            }

            wrapper = null;
            return false;
        }

        public bool TrySetAssignment(int hour, TimeAssignmentDef assignment)
        {
            var schedule = ScheduleRef(_link);
            if (schedule == null || hour < 0 || hour >= schedule.Count)
            {
                return false;
            }

            schedule[hour] = assignment;
            return true;
        }
    }
}
