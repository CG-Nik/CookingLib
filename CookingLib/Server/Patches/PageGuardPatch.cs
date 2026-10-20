using System;
using System.Collections.Generic;
using System.Linq;
using Alta;
using Alta.Cooking;
using Alta.Pages;
using HarmonyLib;

namespace CookingLib
{
    internal static class PageGuardPatch
    {
        internal static void Apply(HarmonyLib.Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(StewRecipePageProvider), nameof(StewRecipePageProvider.GetRandom)),
                prefix: new HarmonyMethod(typeof(PageGuardPatch), nameof(Prefix)));
        }

        static bool Prefix(StewRecipePageProvider __instance, ref PageData __result)
        {
            __result = null;
            try
            {
                List<StewRecipe> vanilla = HashedGeneralValue<StewRecipe>.All.Where(recipe => StewRecipes.Find(recipe) == null).ToList();
                if (vanilla.Count == 0)
                    return false;

                StewRecipe pick = vanilla[UnityEngine.Random.Range(0, vanilla.Count)];
                if (GlobalPagesManager.IsNew(__instance.Identifier, pick.Hash))
                    __result = __instance.GeneratePage(pick);
            }
            catch (Exception e)
            {
                __result = null;
                Safe.Fail(nameof(PageGuardPatch), e);
            }
            return false;
        }
    }
}
