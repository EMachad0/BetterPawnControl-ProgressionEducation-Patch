using System;
using System.Collections;
using System.Collections.Generic;
using BetterPawnControl;
using HarmonyLib;

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
            AccessTools.Field(ScheduleManagerType, "links") ??
            AccessTools.Field(ManagerType, "links");

        public static int GetActivePolicyIdOrDefault(int fallbackPolicyId = 0)
        {
            var activePolicy = GetActivePolicyDelegate?.Invoke();
            return activePolicy == null ? fallbackPolicyId : PolicyIdRef(activePolicy);
        }

        public static IEnumerable<ScheduleLinkWrapper> EnumerateScheduleLinks()
        {
            var links = LinksField?.GetValue(null) as IEnumerable;
            if (links == null)
            {
                yield break;
            }

            foreach (var rawLink in links)
            {
                if (ScheduleLinkWrapper.TryCreate(rawLink, out var wrapper))
                {
                    yield return wrapper;
                }
            }
        }

        private static Func<Policy> BuildGetActivePolicyDelegate()
        {
            var method = AccessTools.Method(ScheduleManagerType, "GetActivePolicy") ??
                         AccessTools.Method(ManagerType, "GetActivePolicy");

            return method == null
                ? null
                : AccessTools.MethodDelegate<Func<Policy>>(method);
        }
    }
}
