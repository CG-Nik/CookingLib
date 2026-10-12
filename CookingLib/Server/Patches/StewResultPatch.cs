using Alta.Cooking;
using Alta.Liquid;
using HarmonyLib;
using UnityEngine;

namespace CookingLib
{
    internal static class StewResultPatch
    {
        internal static void Apply(HarmonyLib.Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(StewManager), "CreateContentWithTint"),
                prefix: new HarmonyMethod(typeof(StewResultPatch), nameof(Prefix)),
                postfix: new HarmonyMethod(typeof(StewResultPatch), nameof(Postfix)));
        }

        static void Prefix(StewRecipe ___currentRecipe, ref Color colorTint)
        {
            LibStewRecipe ours = StewRecipes.Find(___currentRecipe);
            if (ours != null && ours.Spec.Tint.HasValue)
                colorTint = ours.Spec.Tint.Value;
        }

        static void Postfix(StewRecipe ___currentRecipe, ref LiquidContent __result)
        {
            LibStewRecipe ours = StewRecipes.Find(___currentRecipe);
            if (ours != null && ours.Result != null)
                __result = ours.Result.CreateContent();
        }
    }
}
