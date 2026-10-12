using System;
using System.Text;
using Alta;
using Alta.Cooking;
using Alta.Inventory;
using Alta.Networking;

namespace CookingLib
{
    internal static class Names
    {
        internal static string Norm(string name)
        {
            if (name == null)
                return "";

            StringBuilder key = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (c == ' ' || c == '_' || c == '-' || c == '\'')
                    continue;
                key.Append(char.ToLowerInvariant(c));
            }
            return key.ToString();
        }

        internal static T Find<T>(string name) where T : HashedGeneralValue<T>
        {
            string key = Norm(name);
            if (key.Length == 0)
                return null;

            uint.TryParse(key, out uint hash);
            foreach (T value in HashedGeneralValue<T>.All)
            {
                if (value == null)
                    continue;
                if (hash != 0 ? value.Hash == hash : Norm(value.name) == key)
                    return value;
            }
            return null;
        }

        internal static T Need<T>(string name, string kind) where T : HashedGeneralValue<T>
        {
            T value = Find<T>(name);
            if (value == null)
                throw new ArgumentException("no " + kind + " named '" + name + "'");
            return value;
        }

        internal static Item NeedItem(string name)
        {
            Item item = Find<Item>(name);
            if (item != null)
                return item;

            string key = Norm(name);
            foreach (Item candidate in HashedGeneralValue<Item>.All)
            {
                NetworkPrefab prefab = candidate == null ? null : candidate.Prefab;
                if (prefab != null && (Norm(prefab.name) == key || prefab.Hash.ToString() == key))
                    return candidate;
            }
            throw new ArgumentException("no item named '" + name + "'");
        }

        internal static CookableCategoryDefinition NeedCategory(string name)
        {
            string key = Norm(name);
            foreach (CookableCategoryDefinition category in CookableCategoryDefinition.AllCookableItems.Keys)
            {
                if (category != null && Norm(category.name) == key)
                    return category;
            }
            throw new ArgumentException("no cookable category named '" + name + "'");
        }

        internal static NetworkPrefab NeedPrefab(string name)
        {
            PrefabManager.PrepareSpawnSetups();
            string key = Norm(name);
            foreach (NetworkPrefab prefab in PrefabManager.AllPrefabs)
            {
                if (prefab != null && (Norm(prefab.name) == key || prefab.Hash.ToString() == key))
                    return prefab;
            }
            throw new ArgumentException("no stew recipe or prefab named '" + name + "'");
        }
    }
}
