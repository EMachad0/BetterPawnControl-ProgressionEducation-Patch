using System;
using System.Collections;
using System.Collections.Generic;
using BetterPawnControl;
using HarmonyLib;
using Verse;

namespace BetterPawnControlProgressionEducationPatch.Interop.BetterPawnControl
{
    internal static class ScheduleManagerWrapper
    {
        private static readonly Type ScheduleManagerType = AccessTools.TypeByName("BetterPawnControl.ScheduleManager");
        private static readonly Type ManagerType = AccessTools.TypeByName("BetterPawnControl.Manager");

        private static readonly AccessTools.FieldRef<Policy, int> PolicyIdRef =
            AccessTools.FieldRefAccess<Policy, int>("id");

        private static readonly Func<Policy> GetActivePolicyDelegate = BuildGetActivePolicyDelegate();

        private static readonly System.Reflection.FieldInfo LinksField =
            AccessTools.Field(ScheduleManagerType, "links");

        public static int GetActivePolicyIdOrDefault(int fallbackPolicyId = 0)
        {
            var activePolicy = GetActivePolicyDelegate?.Invoke();
            return activePolicy == null ? fallbackPolicyId : PolicyIdRef(activePolicy);
        }

        public static int GetPolicyIdOrDefault(Policy policy, int fallbackPolicyId = 0)
        {
            return policy == null ? fallbackPolicyId : PolicyIdRef(policy);
        }

        public static IEnumerable<ScheduleLinkWrapper> EnumerateScheduleLinks()
        {
            var links = LinksField?.GetValue(null) as IEnumerable;
            foreach (var rawLink in links)
            {
                if (ScheduleLinkWrapper.TryCreate(rawLink, out var wrapper))
                {
                    yield return wrapper;
                }
            }
        }

        internal static ScheduleLinkWrapper GetScheduleLink(Pawn pawn, int policyId, int mapId)
        {
            return ScheduleManagerWrapper
                .EnumerateScheduleLinks()
                .FirstOrFallback(l => l.zone == policyId && l.colonist is not null && pawn.Equals(l.colonist) && l.mapId == mapId);
        }


        public static ScheduleLinkWrapper GetOrCreateScheduleLink(Pawn pawn, int policyId, int mapId)
        {
            var link = GetScheduleLink(pawn, policyId, mapId);
            if (link is not null) return link;

            var links = LinksField?.GetValue(null) as IList;
            var newLink = new ScheduleLink(
                policyId,
                pawn,
                pawn.playerSettings?.AreaRestrictionInPawnCurrentMap,
                pawn.timetable?.times,
                mapId);

            links.Add(newLink);
            return new ScheduleLinkWrapper(newLink);
        }

        private static Func<Policy> BuildGetActivePolicyDelegate()
        {
            var method = AccessTools.Method(ScheduleManagerType, "GetActivePolicy");
            return AccessTools.MethodDelegate<Func<Policy>>(method);
        }
    }
}
