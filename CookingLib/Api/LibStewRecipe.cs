using Alta.Cooking;

namespace CookingLib
{
    /// <summary>A stew recipe a mod registered. Check Error before using it.</summary>
    public class LibStewRecipe
    {
        internal LibStewRecipe(StewRecipeSpec spec)
        {
            Spec = spec;
        }

        /// <summary>The spec it was registered with.</summary>
        public StewRecipeSpec Spec { get; }

        /// <summary>The id from the spec.</summary>
        public string Id => Spec?.Id;

        /// <summary>Why it was refused, or null.</summary>
        public string Error { get; internal set; }

        /// <summary>The game's recipe. Built on the server once the world is up, always null on players' machines.</summary>
        public StewRecipe Recipe { get; internal set; }

        internal LiquidRef Result { get; set; }
    }
}
