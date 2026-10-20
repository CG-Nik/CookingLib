using System.Collections.Generic;
using System.Text;
using Alta.Console;
using Alta.Liquid;
using CookingLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CookingLibExample.Example), "CookingLibExample", "0.1.1", "Shadow")]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace CookingLibExample
{
    public class Example : MelonMod
    {
        static bool commandsAdded;

        public override void OnInitializeMelon()
        {
            CookingLib.Main.Setup += Setup;
        }

        public override void OnUpdate()
        {
            if (commandsAdded || !CookingLib.Main.IsServer || NetworkSceneManager.Current == null)
                return;

            commandsAdded = true;
            CommandService.CommandCollection.ProcessModule(typeof(ExampleCommands), typeof(ExampleCommands).Assembly, null);
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

            Liquids.RegisterServerOnly(new LiquidSpec
            {
                Id = "example.weakskin",
                Name = "Weak Oakskin",
                Look = "Water Liquid Visual Data",
                Color = new Color(0.62f, 0.5f, 0.35f),
                Effects = { new EffectSpec { Effect = "Damage Protection Indirect effect", Strength = 0.35f } }
            });

            Liquids.RegisterServerOnly(new LiquidSpec
            {
                Id = "example.oakskin",
                Name = "Oakskin",
                Look = "Water Liquid Visual Data",
                Color = new Color(0.45f, 0.3f, 0.15f),
                Effects = { new EffectSpec { Effect = "Damage Protection Indirect effect", Strength = 2f } }
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

            Infusions.Register(new InfusionSpec
            {
                Id = "example.weakskininfusion",
                Item = "Copper Ingot",
                OutputLiquid = "example.weakskin"
            });

            Infusions.Register(new InfusionSpec
            {
                Id = "example.oakskininfusion",
                Item = "Iron Ingot",
                OutputLiquid = "example.oakskin"
            });
        }
    }

    [Module("cookinglibexample", "CookingLibExample: find")]
    internal static class ExampleCommands
    {
        [Command("find", "Names the CookingLib liquid in every loaded container, using Liquids.Find")]
        public static string Find()
        {
            StringBuilder text = new StringBuilder();
            foreach (LiquidContainer container in Object.FindObjectsOfType<LiquidContainer>())
            {
                LibLiquid liquid = Liquids.Find(container.ContainedLiquid);
                if (liquid != null)
                    text.Append(container.transform.root.name).Append(" holds ").Append(liquid.Id).Append('\n');
            }
            return text.Length == 0 ? "no container holds a CookingLib liquid" : text.ToString();
        }
    }
}
