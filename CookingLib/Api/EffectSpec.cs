namespace CookingLib
{
    /// <summary>One effect and its strength.</summary>
    public class EffectSpec
    {
        /// <summary>The game's effect name, copied from the dump.</summary>
        public string Effect;

        /// <summary>Strength multiplier, from minus 100 to 100.</summary>
        public float Strength = 1f;

        /// <summary>The duration of the effect, in seconds. 0s means instant.</summary>
        public float durationInSeconds = 0f;
    }
}
