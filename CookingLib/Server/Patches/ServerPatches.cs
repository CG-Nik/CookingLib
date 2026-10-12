using System;
using HarmonyLib;

namespace CookingLib
{
    internal static class ServerPatches
    {
        internal static bool PagesGuarded { get; private set; }

        internal static void Apply(HarmonyLib.Harmony harmony)
        {
            PagesGuarded = Run(PageGuardPatch.Apply, harmony);
            Run(PotBasePatch.Apply, harmony);
            Run(StewResultPatch.Apply, harmony);
            Run(InfusionPatch.Apply, harmony);
        }

        static bool Run(Action<HarmonyLib.Harmony> apply, HarmonyLib.Harmony harmony)
        {
            try
            {
                apply(harmony);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(apply.Method.DeclaringType.Name + " failed to apply: " + e);
                return false;
            }
        }
    }
}
