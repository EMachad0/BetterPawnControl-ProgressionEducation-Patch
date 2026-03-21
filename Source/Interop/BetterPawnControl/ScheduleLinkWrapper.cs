using System.Collections.Generic;
using BetterPawnControl;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl
{
    internal sealed class ScheduleLinkWrapper
    {
        internal static readonly AccessTools.FieldRef<Link, int> ZoneRef =
            AccessTools.FieldRefAccess<Link, int>("zone");

        internal static readonly AccessTools.FieldRef<Link, int> MapIdRef =
            AccessTools.FieldRefAccess<Link, int>("mapId");

        internal static readonly AccessTools.FieldRef<ScheduleLink, Pawn> ColonistRef =
            AccessTools.FieldRefAccess<ScheduleLink, Pawn>("colonist");

        internal static readonly AccessTools.FieldRef<ScheduleLink, Area> AreaRef =
            AccessTools.FieldRefAccess<ScheduleLink, Area>("area");

        internal static readonly AccessTools.FieldRef<ScheduleLink, List<TimeAssignmentDef>> ScheduleRef =
            AccessTools.FieldRefAccess<ScheduleLink, List<TimeAssignmentDef>>("schedule");

        private readonly ScheduleLink _link;

        public int zone
        {
            get => ZoneRef(_link);
            set => ZoneRef(_link) = value;
        }

        public int mapId
        {
            get => MapIdRef(_link);
            set => MapIdRef(_link) = value;
        }

        public Pawn colonist
        {
            get => ColonistRef(_link);
            set => ColonistRef(_link) = value;
        }

        public Area area
        {
            get => AreaRef(_link);
            set => AreaRef(_link) = value;
        }

        public List<TimeAssignmentDef> schedule
        {
            get => ScheduleRef(_link);
            set => ScheduleRef(_link) = value;
        }

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
