using System.Collections.Generic;
using UnityEngine;

namespace CookingLib
{
    /// <summary>Everything a mod says about a new liquid.</summary>
    public class LiquidSpec
    {
        /// <summary>Unique id written as author.thing, for example shadow.blackdye.</summary>
        public string Id;

        /// <summary>Display name for logs and the dump. Falls back to the id.</summary>
        public string Name;

        /// <summary>A vanilla liquid look, copied from the dump.</summary>
        public string Look;

        /// <summary>Tint over the look. Every channel from 0 to 1.</summary>
        public Color Color = Color.white;

        /// <summary>Effects a drinker gets.</summary>
        public List<EffectSpec> Effects = new List<EffectSpec>();

        /// <summary>Vanilla chunk looks floating in the liquid, copied from the dump.</summary>
        public List<string> Chunks = new List<string>();

        /// <summary>True lets a splash apply the effects, like a teleport potion.</summary>
        public bool ConsumableThroughSkin;

        /// <summary>How strong a splash is when ConsumableThroughSkin is true.</summary>
        public float SplashStrength = 1f;

        /// <summary>Amount of thirst the liquid will give (100 max) [Optional][null = 40f][supports negative] </summary>
        public float? Thirst { get; set; }
    }
}
