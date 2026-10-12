using System;
using Alta.Liquid;

namespace CookingLib
{
    internal class LiquidRef
    {
        LiquidRef(LibLiquid lib, LiquidDefinition vanilla)
        {
            Lib = lib;
            Vanilla = vanilla;
        }

        internal LibLiquid Lib { get; }

        internal LiquidDefinition Vanilla { get; }

        internal LiquidDefinition Definition => Lib != null ? Lib.Definition : Vanilla;

        internal static LiquidRef Resolve(string name)
        {
            LibLiquid lib = Liquids.Get(name);
            if (lib != null)
                return new LiquidRef(lib, null);

            LiquidDefinition vanilla = Names.Find<LiquidDefinition>(name);
            if (vanilla == null)
                throw new ArgumentException("no liquid named '" + name + "'");
            return new LiquidRef(null, vanilla);
        }

        internal bool Matches(LiquidContent content)
        {
            if (content == null)
                return false;
            if (Lib != null && !Lib.IsShared)
                return content.IsCustom && Liquids.FindServerOnly(content.Data) == Lib;
            return !content.IsCustom && ReferenceEquals(content.Liquid, Definition);
        }

        internal LiquidContent CreateContent() => Lib != null ? Lib.CreateContent() : Vanilla.GetAsLiquidContent();
    }
}
