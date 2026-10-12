using System.Collections.Generic;
using CookingLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CookingLibExample.Example), "CookingLibExample", "0.1.0", "Shadow")]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace CookingLibExample
{
    public class Example : MelonMod
    {
        public override void OnInitializeMelon()
        {
            CookingLib.Main.Setup += Setup;
        }

        static void Setup()
        {
            Liquids.RegisterServerOnly(new LiquidSpec
            {
                Id = "example.ashwater",
                Name = "Ash Water",
                Look = "Water Liquid Visual Data",
                Color = new Color(0.55f, 0.53f, 0.5f),
                Effects = { new EffectSpec { Effect = "Heal", Strength = 0.5f } },
                Chunks = { "Apple Ripe Chunk Visual" }
            });

            Liquids.RegisterShared(60001, new LiquidSpec
            {
                Id = "example.blackdye",
                Name = "Black Dye",
                Look = "Water Liquid Visual Data",
                Color = new Color(0.08f, 0.08f, 0.08f)
            });

            StewRecipes.Register(new StewRecipeSpec
            {
                Id = "example.ashstew",
                Look = "Vegetable Ragu",
                BaseLiquids = new List<string> { "example.ashwater" },
                Slots =
                {
                    new SlotSpec { Category = "Mushrooms" },
                    new SlotSpec { Category = "Mushrooms" },
                    new SlotSpec { Category = "Vegetables Category" },
                    new SlotSpec { Category = "Vegetables Category" }
                },
                ExtraEffects = { new EffectSpec { Effect = "Heal", Strength = 1f } },
                Tint = new Color(0.45f, 0.42f, 0.38f)
            });

            Infusions.Register(new InfusionSpec
            {
                Id = "example.coaldye",
                Item = "Coal",
                OutputLiquid = "example.blackdye"
            });

            Infusions.Register(new InfusionSpec
            {
                Id = "example.ashinfusion",
                Item = "Coal Ash",
                OutputLiquid = "example.ashwater"
            });
        }
    }
}
