using System;
using System.IO;
using System.Linq;
using System.Text;
using Alta;
using Alta.Alchemy;
using Alta.Console;
using Alta.Cooking;
using Alta.Inventory;
using Alta.Liquid;
using MelonLoader.Utils;

namespace CookingLib
{
    [Module("cookinglib", "CookingLib: status, dump")]
    internal static class CookingLibCommands
    {
        [Command("status", "Registered liquids, stew recipes and infusions, and why any were refused")]
        public static string Status()
        {
            StringBuilder text = new StringBuilder("CookingLib " + Main.Version + "\n");

            text.Append("liquids: ").Append(Liquids.All.Count).Append('\n');
            foreach (LibLiquid liquid in Liquids.All)
            {
                string state = liquid.Error != null ? "refused: " + liquid.Error : liquid.IsShared ? "shared as " + liquid.Definition.Hash : "server only";
                text.Append("  ").Append(liquid.Id).Append("  ").Append(state).Append('\n');
            }

            text.Append("stew recipes: ").Append(StewRecipes.All.Count).Append('\n');
            foreach (LibStewRecipe recipe in StewRecipes.All)
            {
                string state = recipe.Error != null ? "refused: " + recipe.Error : recipe.Recipe != null ? "recipe " + recipe.Recipe.Hash : "waiting for the world";
                text.Append("  ").Append(recipe.Id).Append("  ").Append(state).Append('\n');
            }

            text.Append("infusions: ").Append(Infusions.All.Count).Append('\n');
            foreach (LibInfusion infusion in Infusions.All)
            {
                string state = infusion.Error != null ? "refused: " + infusion.Error : infusion.IsReady ? "ready" : "waiting for the world";
                text.Append("  ").Append(infusion.Id).Append("  ").Append(state).Append('\n');
            }

            return text.ToString();
        }

        [Command("dump", "Writes every name a spec can use to UserData CookingLib-dump.txt")]
        public static string Dump()
        {
            try
            {
                string path = Path.Combine(MelonEnvironment.UserDataDirectory, "CookingLib-dump.txt");
                File.WriteAllText(path, BuildDump());
                return "wrote " + path;
            }
            catch (Exception e)
            {
                return "dump failed: " + e.Message;
            }
        }

        internal static void Register()
        {
            try
            {
                if (CommandService.CommandCollection.ProcessModule(typeof(CookingLibCommands), typeof(CookingLibCommands).Assembly, null) == null)
                    Log.Warn("console commands were not registered");
            }
            catch (Exception e)
            {
                Log.Error("console commands failed to register: " + e);
            }
        }

        static string BuildDump()
        {
            StringBuilder text = new StringBuilder("CookingLib " + Main.Version + " dump. Copy names from here into specs.\n\n");
            Section<LiquidDefinition>(text, "Liquids");
            Section<LiquidVisualData>(text, "Liquid looks");
            Section<LiquidVisualChunkDefinition>(text, "Chunks");
            Section<EffectDefinition>(text, "Effects");
            Section<StewRecipe>(text, "Stew recipes");

            text.Append("== Cookable categories ==\n");
            foreach (CookableCategoryDefinition category in CookableCategoryDefinition.AllCookableItems.Keys.Where(c => c != null).OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase))
                text.Append(category.name).Append('\n');

            text.Append("\n== Items ==\n");
            foreach (Item item in HashedGeneralValue<Item>.All.Where(i => i != null).OrderBy(i => i.name, StringComparer.OrdinalIgnoreCase))
            {
                CookableItem cookable = item.GetComponent<CookableItem>();
                text.Append(item.name).Append("  ").Append(item.Hash);
                if (item.Prefab != null && item.Prefab.name != item.name)
                    text.Append("  prefab ").Append(item.Prefab.name);
                if (cookable != null && cookable.GetType() == typeof(CookableItem))
                    text.Append("  stew ingredient");
                text.Append('\n');
            }
            return text.ToString();
        }

        static void Section<T>(StringBuilder text, string title) where T : HashedGeneralValue<T>
        {
            text.Append("== ").Append(title).Append(" ==\n");
            foreach (T value in HashedGeneralValue<T>.All.Where(v => v != null).OrderBy(v => v.name, StringComparer.OrdinalIgnoreCase))
                text.Append(value.name).Append("  ").Append(value.Hash).Append('\n');
            text.Append('\n');
        }
    }
}
