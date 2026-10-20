using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Alta;
using Alta.Alchemy;
using Alta.Console;
using Alta.Cooking;
using Alta.Inventory;
using Alta.Liquid;
using Alta.Networking;
using MelonLoader.Utils;
using UnityEngine;

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

            text.Append("infusions: ").Append(Infusions.All.Count).Append(", ").Append(InfusionPatch.Done).Append(" done, ")
                .Append(InfusionPatch.Refused).Append(" refused on endless sources and rivers\n");
            foreach (LibInfusion infusion in Infusions.All)
            {
                string state = infusion.Error != null ? "refused: " + infusion.Error : infusion.IsReady ? "ready" : "waiting for the world";
                text.Append("  ").Append(infusion.Id).Append("  ").Append(state).Append('\n');
            }

            text.Append("patch failures: ").Append(Safe.Failures).Append('\n');
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
                Log.Error("dump failed: " + e);
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
            OurLiquids(text);
            Section<LiquidVisualData>(text, "Liquid looks");
            Section<LiquidVisualChunkDefinition>(text, "Chunks");
            Effects(text);
            Section<StewRecipe>(text, "Stew recipes");
            EndlessSources(text);

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

        static void OurLiquids(StringBuilder text)
        {
            text.Append("== CookingLib liquids (key server only ones by id) ==\n");
            foreach (LibLiquid liquid in Liquids.All.Where(l => l.Error == null).OrderBy(l => l.Id, StringComparer.OrdinalIgnoreCase))
                text.Append(liquid.Id).Append("  ").Append(liquid.IsShared ? "shared " + liquid.Definition.Hash : "server only").Append('\n');
            text.Append('\n');
        }

        static void Effects(StringBuilder text)
        {
            Dictionary<EffectDefinition, List<string>> uses = EffectUses();
            text.Append("== Effects (settings, then the strengths the game uses) ==\n");
            foreach (EffectDefinition effect in HashedGeneralValue<EffectDefinition>.All.Where(e => e != null).OrderBy(e => e.name, StringComparer.OrdinalIgnoreCase))
            {
                text.Append(effect.name).Append("  ").Append(effect.Hash).Append("  ").Append(effect.GetType().Name).Append("  ").Append(Settings(effect)).Append('\n');
                if (uses.TryGetValue(effect, out List<string> list))
                {
                    foreach (string use in list)
                        text.Append("    ").Append(use).Append('\n');
                }
            }
            text.Append('\n');
        }

        static string Settings(EffectDefinition effect)
        {
            List<string> parts = new List<string>();
            for (Type type = effect.GetType(); type != typeof(EffectDefinition); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsPublic || field.IsDefined(typeof(SerializeField), false))
                        parts.Add(field.Name + " " + Show(field.GetValue(effect)));
                }
            }
            return string.Join(", ", parts.ToArray());
        }

        static string Show(object value)
        {
            if (value is HashedGeneralValue hashed)
                return hashed == null ? "none" : hashed.name + " " + hashed.Hash;
            if (value is UnityEngine.Object asset)
                return asset == null ? "none" : asset.name;
            return value == null ? "none" : value.ToString();
        }

        static Dictionary<EffectDefinition, List<string>> EffectUses()
        {
            Dictionary<EffectDefinition, List<string>> uses = new Dictionary<EffectDefinition, List<string>>();
            foreach (LiquidDefinition liquid in HashedGeneralValue<LiquidDefinition>.All.Where(l => l != null && l.Data != null))
                AddUses(uses, liquid.Data.Effects, "liquid " + liquid.name);
            foreach (LibLiquid liquid in Liquids.All.Where(l => l.Error == null && !l.IsShared))
                AddUses(uses, liquid.Data.Effects, "liquid " + liquid.Id);
            foreach (StewRecipe recipe in HashedGeneralValue<StewRecipe>.All.Where(r => r != null))
            {
                AddUses(uses, recipe.ExtraEffects, "recipe " + recipe.name + " extra");
                AddUses(uses, recipe.EffectModifiers, "recipe " + recipe.name + " modifier");
            }
            foreach (Item item in HashedGeneralValue<Item>.All.Where(i => i != null))
            {
                CookableItem cookable = item.GetComponent<CookableItem>();
                if (cookable != null && cookable.GetType() == typeof(CookableItem))
                    AddUses(uses, cookable.Effects, "ingredient " + item.name);
            }
            return uses;
        }

        static void AddUses(Dictionary<EffectDefinition, List<string>> uses, IEnumerable<EffectDefinitionApplication> effects, string where)
        {
            if (effects == null)
                return;

            foreach (EffectDefinitionApplication effect in effects)
            {
                if (effect == null || effect.EffectToModify == null)
                    continue;
                if (!uses.TryGetValue(effect.EffectToModify, out List<string> list))
                    uses[effect.EffectToModify] = list = new List<string>();
                list.Add(where + "  " + effect.StrengthMultiplier);
            }
        }

        static void EndlessSources(StringBuilder text)
        {
            text.Append("== Endless liquid sources (infusions skip these) ==\n");
            PrefabManager.PrepareSpawnSetups();
            foreach (NetworkPrefab prefab in PrefabManager.AllPrefabs.Where(p => p != null).OrderBy(p => p.name, StringComparer.OrdinalIgnoreCase))
            {
                if (prefab.GetComponentsInChildren<LiquidContainer>(true).Any(InfusionPatch.IsEndless))
                    text.Append(prefab.name).Append("  ").Append(prefab.Hash).Append('\n');
            }
            text.Append('\n');
        }
    }
}
