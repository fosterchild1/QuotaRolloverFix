using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using QuotaRollover.Patches;
using UnityEngine;

namespace QuotaRollover
{
    [BepInPlugin("fosterchild1.QuotraRolloverFix", "Quota Rollover Fix", "1.0.0")]
    public class QuotaRolloverBase : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("fosterchild1.QuotraRolloverFix");

        private static QuotaRolloverBase Instance;

        public static ManualLogSource logger;

        private void Awake()
        {
            if ((Object)(object)Instance == (Object)null)
            {
                Instance = this;
            }

            harmony.PatchAll(typeof(QuotaRolloverBase));
            harmony.PatchAll(typeof(TimeOfDayPatch));
        }
    }
}
namespace QuotaRollover.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPrefix]
        [HarmonyAfter(new string[] { })]
        private static bool GetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota, out int __state)
        {
            if (TimeOfDay.Instance.daysUntilDeadline < 1)
            {
                __state = ___quotaFulfilled - ___profitQuota;
                return true;
            }
            __state = ___quotaFulfilled;
            return false;
        }

        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPostfix]
        [HarmonyBefore(new string[] { })]
        private static void SetQuotaFulfilledHost(ref int ___quotaFulfilled, int __state)
        {
            ___quotaFulfilled = __state;
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPrefix]
        private static void GetNewQuotaFulfilledClient(ref int ___quotaFulfilled, ref int ___profitQuota, out int __state)
        {
            __state = ___quotaFulfilled - ___profitQuota;

        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPostfix]
        private static void SetNewQuotaFulfiledClient(ref int ___quotaFulfilled, int __state)
        {
            if (___quotaFulfilled == 0)
            {
                ___quotaFulfilled = __state;
            }
        }
    }
}