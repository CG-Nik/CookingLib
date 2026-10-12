using Alta.Liquid;

namespace CookingLib
{
    /// <summary>A liquid a mod registered. Check Error before using it.</summary>
    public class LibLiquid
    {
        internal LibLiquid(LiquidSpec spec, bool isShared)
        {
            Spec = spec;
            IsShared = isShared;
        }

        /// <summary>The spec it was registered with.</summary>
        public LiquidSpec Spec { get; }

        /// <summary>The id from the spec.</summary>
        public string Id => Spec?.Id;

        /// <summary>True for a shared liquid, false for a server only one.</summary>
        public bool IsShared { get; }

        /// <summary>Why it was refused, or null when it registered.</summary>
        public string Error { get; internal set; }

        /// <summary>The liquid data every container holding this liquid points at.</summary>
        public LiquidData Data { get; internal set; }

        /// <summary>A shared liquid's registered definition, or a server only liquid's private stand in.</summary>
        public LiquidDefinition Definition { get; internal set; }

        internal string Fingerprint { get; set; }

        internal LiquidContent CreateContent()
        {
            if (IsShared)
                return Definition.GetAsLiquidContent();
            return new LiquidContent { IsCustom = true, Data = Data };
        }
    }
}
