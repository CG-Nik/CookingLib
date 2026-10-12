using System.Collections.Generic;
using System.Reflection;
using Alta;

namespace CookingLib
{
    internal static class Registry
    {
        const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        static readonly FieldInfo hashField = typeof(HashedGeneralValue).GetField("hash", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void SetHash(HashedGeneralValue value, uint hash) => hashField.SetValue(value, (int)hash);

        internal static bool Add<T>(T value) where T : HashedGeneralValue<T>
        {
            HashedGeneralValue<T>.CheckItems();
            Dictionary<uint, T> items = (Dictionary<uint, T>)typeof(HashedGeneralValue<T>).GetField("items", PrivateStatic).GetValue(null);
            if (items.ContainsKey(value.Hash))
                return false;

            items.Add(value.Hash, value);
            typeof(HashedGeneralValue<T>).GetField("itemsByName", PrivateStatic).SetValue(null, null);
            return true;
        }
    }
}
