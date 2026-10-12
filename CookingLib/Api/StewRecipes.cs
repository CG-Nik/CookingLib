using System;
using System.Collections.Generic;
using System.Linq;
using Alta.Cooking;
using UnityEngine;

namespace CookingLib
{
    /// <summary>Registers stew recipes. They run on the server only, so vanilla players can join.</summary>
    public static class StewRecipes
    {
        static readonly List<LibStewRecipe> all = new List<LibStewRecipe>();
        static bool worldUp;

        /// <summary>Every stew recipe mods tried to register, refused ones included.</summary>
        public static IReadOnlyList<LibStewRecipe> All => all;

        /// <summary>Registers a stew recipe. Call it from Main.Setup. The server builds it once the world is up.</summary>
        public static LibStewRecipe Register(StewRecipeSpec spec)
        {
            LibStewRecipe recipe = new LibStewRecipe(spec);
            try
            {
                Validate(spec);
            }
            catch (ArgumentException e)
            {
                recipe.Error = e.Message;
            }

            all.Add(recipe);
            if (recipe.Error != null)
                Log.Warn("stew recipe " + recipe.Id + " refused: " + recipe.Error);
            else if (worldUp)
                Build(recipe);
            return recipe;
        }

        /// <summary>The stew recipe with this id that was not refused, or null.</summary>
        public static LibStewRecipe Get(string id)
        {
            foreach (LibStewRecipe recipe in all)
            {
                if (recipe.Error == null && string.Equals(recipe.Id, id, StringComparison.OrdinalIgnoreCase))
                    return recipe;
            }
            return null;
        }

        internal static LibStewRecipe Find(StewRecipe recipe)
        {
            if (ReferenceEquals(recipe, null))
                return null;

            foreach (LibStewRecipe ours in all)
            {
                if (ReferenceEquals(ours.Recipe, recipe))
                    return ours;
            }
            return null;
        }

        internal static void ResolveAll()
        {
            worldUp = true;
            foreach (LibStewRecipe recipe in all)
            {
                if (recipe.Error == null)
                    Build(recipe);
            }
        }

        static void Build(LibStewRecipe recipe)
        {
            try
            {
                if (!ServerPatches.PagesGuarded)
                    throw new ArgumentException("the page guard patch failed, so recipes stay off");

                StewRecipeFactory.Build(recipe);
                Log.Info("stew recipe " + recipe.Id + " ready as recipe " + recipe.Recipe.Hash);
            }
            catch (Exception e)
            {
                recipe.Error = e is ArgumentException ? e.Message : e.ToString();
                Log.Warn("stew recipe " + recipe.Id + " refused: " + recipe.Error);
            }
        }

        static void Validate(StewRecipeSpec spec)
        {
            if (spec == null)
                throw new ArgumentException("spec is null");
            if (string.IsNullOrEmpty(spec.Id) || spec.Id.IndexOf('.') <= 0)
                throw new ArgumentException("id must look like author.thing");
            if (Get(spec.Id) != null)
                throw new ArgumentException("id is already registered");
            if (string.IsNullOrEmpty(spec.Look))
                throw new ArgumentException("Look is empty");
            if (spec.BaseLiquids == null || spec.BaseLiquids.Count == 0 || spec.BaseLiquids.Any(string.IsNullOrEmpty))
                throw new ArgumentException("BaseLiquids is empty or holds an empty name");
            if (spec.Slots == null || spec.Slots.Count == 0)
                throw new ArgumentException("Slots is empty");

            foreach (SlotSpec slot in spec.Slots)
            {
                bool hasItems = slot != null && slot.Items != null && slot.Items.Count > 0;
                bool hasCategory = slot != null && !string.IsNullOrEmpty(slot.Category);
                if (hasItems == hasCategory)
                    throw new ArgumentException("every slot needs Items or a Category, not both");
            }

            if (spec.ExtraEffects == null || spec.EffectModifiers == null || spec.ExtraEffects.Contains(null) || spec.EffectModifiers.Contains(null))
                throw new ArgumentException("ExtraEffects and EffectModifiers can't be null or hold a null effect");
            if (spec.ExtraEffects.Count > 3)
                throw new ArgumentException("a stew shows at most 3 extra effects");
            if (spec.ExtraEffects.Concat(spec.EffectModifiers).Any(effect => Mathf.Abs(effect.Strength) > 100f))
                throw new ArgumentException("effect strengths must be minus 100 to 100");
            if (spec.Tint.HasValue && !LiquidFactory.IsValidColor(spec.Tint.Value))
                throw new ArgumentException("tint channels must be 0 to 1");

            if (spec.Slots.Count != 4)
                Log.Warn("stew recipe " + spec.Id + " has " + spec.Slots.Count + " slots, the usual pot only checks recipes at 4 ingredients");
        }
    }
}
