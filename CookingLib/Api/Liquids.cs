using System;
using System.Collections.Generic;
using Alta;
using Alta.Liquid;

namespace CookingLib
{
    /// <summary>Registers liquids and puts them into containers.</summary>
    public static class Liquids
    {
        static readonly List<LibLiquid> all = new List<LibLiquid>();

        /// <summary>Every liquid mods tried to register, refused ones included.</summary>
        public static IReadOnlyList<LibLiquid> All => all;

        /// <summary>Registers a liquid only the server knows about. Players get it by value, so vanilla players can still join. Call it from Main.Setup.</summary>
        public static LibLiquid RegisterServerOnly(LiquidSpec spec) => Register(spec, 0, false);

        /// <summary>Registers a real liquid under a number from 1 to 65535. The server and every player need CookingLib and the calling mod, so call it from Main.Setup on both sides.</summary>
        public static LibLiquid RegisterShared(uint hash, LiquidSpec spec) => Register(spec, hash, true);

        /// <summary>The liquid with this id that was not refused, or null.</summary>
        public static LibLiquid Get(string id)
        {
            foreach (LibLiquid liquid in all)
            {
                if (liquid.Error == null && string.Equals(liquid.Id, id, StringComparison.OrdinalIgnoreCase))
                    return liquid;
            }
            return null;
        }

        /// <summary>Server only. Pours a registered liquid into a container that is empty or already holds it. The amount has to fit.</summary>
        public static bool Fill(ILiquidContainer container, string id, int amount)
        {
            LibLiquid liquid = Get(id);
            if (!Main.IsServer || liquid == null || container == null || GlobalLiquidContainer.Instance == null)
                return false;
            return GlobalLiquidContainer.FillLiquidIntoContainer(liquid.CreateContent(), container, amount, true);
        }

        /// <summary>The registered liquid in this content, for example container.ContainedLiquid, or null for vanilla liquids, stews and anything else. Server only liquids match by value, so they still match after a restart.</summary>
        public static LibLiquid Find(LiquidContent content)
        {
            if (content == null)
                return null;
            if (content.IsCustom)
                return FindServerOnly(content.Data);

            foreach (LibLiquid liquid in all)
            {
                if (liquid.Error == null && liquid.IsShared && ReferenceEquals(liquid.Definition, content.Liquid))
                    return liquid;
            }
            return null;
        }

        internal static LibLiquid FindServerOnly(LiquidData data)
        {
            if (data == null)
                return null;

            foreach (LibLiquid liquid in all)
            {
                if (liquid.Error == null && !liquid.IsShared && ReferenceEquals(liquid.Data, data))
                    return liquid;
            }

            string fingerprint = LiquidFactory.Fingerprint(data);
            foreach (LibLiquid liquid in all)
            {
                if (liquid.Error == null && !liquid.IsShared && liquid.Fingerprint == fingerprint)
                    return liquid;
            }
            return null;
        }

        static LibLiquid Register(LiquidSpec spec, uint hash, bool shared)
        {
            LibLiquid liquid = new LibLiquid(spec, shared);
            try
            {
                Validate(spec);
                LiquidData data = LiquidFactory.BuildData(spec);
                if (shared)
                {
                    LiquidDefinition definition = LiquidFactory.BuildDefinition(spec.Name ?? spec.Id, data, hash);
                    SharedLiquidRegistrar.Register(definition);
                    liquid.Definition = definition;
                }
                else
                {
                    string fingerprint = LiquidFactory.Fingerprint(data);
                    CheckLookalikes(fingerprint);
                    liquid.Fingerprint = fingerprint;
                    liquid.Definition = LiquidFactory.BuildDefinition(spec.Name ?? spec.Id, data, LiquidFactory.WaterHash);
                }
                liquid.Data = data;
            }
            catch (Exception e)
            {
                liquid.Error = e is ArgumentException ? e.Message : e.ToString();
            }

            all.Add(liquid);
            if (liquid.Error != null)
                Log.Warn("liquid " + liquid.Id + " refused: " + liquid.Error);
            else
                Log.Info("liquid " + liquid.Id + " registered, " + (shared ? "shared as " + hash : "server only"));
            return liquid;
        }

        static void Validate(LiquidSpec spec)
        {
            if (spec == null)
                throw new ArgumentException("spec is null");
            if (string.IsNullOrEmpty(spec.Id) || spec.Id.IndexOf('.') <= 0)
                throw new ArgumentException("id must look like author.thing");
            if (Get(spec.Id) != null)
                throw new ArgumentException("id is already registered");
            if (spec.Effects == null || spec.Chunks == null || spec.Effects.Contains(null))
                throw new ArgumentException("Effects and Chunks can't be null or hold a null effect");
            if (!LiquidFactory.IsValidColor(spec.Color))
                throw new ArgumentException("colour channels must be 0 to 1");

            foreach (EffectSpec effect in spec.Effects)
            {
                if (!LiquidFactory.IsWithin100(effect.Strength))
                    throw new ArgumentException("effect strengths must be minus 100 to 100");
            }
            if (spec.Thirst.HasValue && !LiquidFactory.IsWithin100(spec.Thirst.Value))
                throw new ArgumentException("Thirst must be minus 100 to 100");
        }

        static void CheckLookalikes(string fingerprint)
        {
            foreach (LibLiquid other in all)
            {
                if (other.Error == null && other.Fingerprint == fingerprint)
                    throw new ArgumentException("looks exactly like " + other.Id + ", change its colour, look, effects or chunks");
            }
            foreach (LiquidDefinition vanilla in HashedGeneralValue<LiquidDefinition>.All)
            {
                if (vanilla != null && vanilla.Data != null && LiquidFactory.Fingerprint(vanilla.Data) == fingerprint)
                    throw new ArgumentException("looks exactly like the game's " + vanilla.name + ", change its colour, look, effects or chunks");
            }
        }
    }
}
