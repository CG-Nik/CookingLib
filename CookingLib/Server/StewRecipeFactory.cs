using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alta.Alchemy;
using Alta.Cooking;
using Alta.Inventory;
using Alta.Liquid;
using UnityEngine;

namespace CookingLib
{
    internal static class StewRecipeFactory
    {
        const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        static readonly FieldInfo lookField = typeof(StewRecipe).GetField("baseVersionHash", PrivateInstance);
        static readonly FieldInfo liquidsField = typeof(StewRecipe).GetField("possibleLiquids", PrivateInstance);
        static readonly FieldInfo slotsField = typeof(StewRecipe).GetField("stewIngredients", PrivateInstance);
        static readonly FieldInfo extraField = typeof(StewRecipe).GetField("extraEffects", PrivateInstance);
        static readonly FieldInfo modifiersField = typeof(StewRecipe).GetField("effectModifiers", PrivateInstance);
        static readonly FieldInfo cookablesField = typeof(CookableCollectionDefinition).GetField("cookables", PrivateInstance);

        internal static void Build(LibStewRecipe lib)
        {
            StewRecipeSpec spec = lib.Spec;
            uint look = Look(spec.Look);
            List<LiquidDefinition> liquids = spec.BaseLiquids.Select(name => LiquidRef.Resolve(name).Definition).ToList();
            LiquidRef result = string.IsNullOrEmpty(spec.ResultLiquid) ? null : LiquidRef.Resolve(spec.ResultLiquid);

            List<EffectDefinitionApplication> modifiers = new List<EffectDefinitionApplication>();
            foreach (EffectSpec effect in spec.EffectModifiers)
                AddOnce(modifiers, effect, "modifier");

            List<EffectDefinitionApplication> extra = new List<EffectDefinitionApplication>();
            foreach (EffectSpec effect in spec.ExtraEffects)
            {
                EffectDefinition definition = AddOnce(extra, effect, "extra effect");
                if (!modifiers.Any(modifier => modifier.EffectToModify == definition))
                    modifiers.Add(new EffectDefinitionApplication(definition, 1f));
            }

            List<StewIngredientBase> slots = new List<StewIngredientBase>();
            for (int i = 0; i < spec.Slots.Count; i++)
                slots.Add(Slot(spec.Id + " slot " + (i + 1), spec.Slots[i]));

            StewRecipe recipe = ScriptableObject.CreateInstance<StewRecipe>();
            recipe.name = spec.Id;
            Registry.SetHash(recipe, HashFor(spec.Id));
            lookField.SetValue(recipe, look);
            liquidsField.SetValue(recipe, liquids);
            slotsField.SetValue(recipe, slots);
            extraField.SetValue(recipe, extra.ToArray());
            modifiersField.SetValue(recipe, modifiers.ToArray());
            if (!Registry.Add(recipe))
                throw new ArgumentException("recipe number " + recipe.Hash + " is taken, pick another id");

            lib.Recipe = recipe;
            lib.Result = result;
        }

        static uint Look(string name)
        {
            StewRecipe vanilla = Names.Find<StewRecipe>(name);
            if (vanilla != null && StewRecipes.Find(vanilla) == null)
                return vanilla.BaseVersionHash;
            return Names.NeedPrefab(name).Hash;
        }

        static EffectDefinition AddOnce(List<EffectDefinitionApplication> list, EffectSpec effect, string kind)
        {
            EffectDefinition definition = Names.Need<EffectDefinition>(effect.Effect, "effect");
            if (list.Any(existing => existing.EffectToModify == definition))
                throw new ArgumentException(kind + " " + definition.name + " is listed twice");
            list.Add(new EffectDefinitionApplication(definition, effect.Strength));
            return definition;
        }

        static StewIngredientBase Slot(string name, SlotSpec slot)
        {
            if (!string.IsNullOrEmpty(slot.Category))
                return Names.NeedCategory(slot.Category);

            List<CookableItem> cookables = new List<CookableItem>();
            foreach (string itemName in slot.Items)
            {
                Item item = Names.NeedItem(itemName);
                CookableItem cookable = item.GetComponent<CookableItem>();
                if (cookable == null || cookable.GetType() != typeof(CookableItem))
                    throw new ArgumentException(item.name + " can't go in a stew");
                if (!cookables.Contains(cookable))
                    cookables.Add(cookable);
            }

            CookableCollectionDefinition collection = ScriptableObject.CreateInstance<CookableCollectionDefinition>();
            collection.name = name;
            cookablesField.SetValue(collection, cookables);
            return collection;
        }

        static uint HashFor(string id)
        {
            uint hash = 2166136261;
            foreach (char c in id.ToLowerInvariant())
            {
                hash ^= c;
                hash *= 16777619;
            }
            hash &= 0x7FFFFFFF;
            return hash <= 65535 ? hash + 65536 : hash;
        }
    }
}
