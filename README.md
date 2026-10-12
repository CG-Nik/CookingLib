# CookingLib

A library for A Township Tale mods. Other mods use it to add liquids, stew recipes and infusions (an item dropped into a liquid turns it into another liquid). On its own it adds nothing.

## Install

Needs MelonLoader. Put `CookingLib.dll` in the `Mods` folder of the server. Players only need it when a mod they play with adds shared liquids:

| A mod adds | Server needs | Players need |
|---|---|---|
| Server only liquids, stew recipes, infusions | CookingLib and the mod | nothing, vanilla players can join |
| Shared liquids | CookingLib and the mod | CookingLib and the mod |

## For modders

Reference `CookingLib.dll` (keep `CookingLib.xml` next to it for tooltips) and register everything from `CookingLib.Main.Setup`:

```csharp
public override void OnInitializeMelon()
{
    CookingLib.Main.Setup += Setup;
}

static void Setup()
{
    Liquids.RegisterServerOnly(new LiquidSpec
    {
        Id = "you.ashwater",
        Look = "Water Liquid Visual Data",
        Color = new Color(0.55f, 0.53f, 0.5f),
        Effects = { new EffectSpec { Effect = "Heal", Strength = 0.5f } }
    });
}
```

A full example with every kind is in `Examples/CookingLibExample`.

### Liquids

- `Liquids.RegisterServerOnly(spec)` makes a liquid only the server knows. Players receive it by value, so vanilla players see and drink it. It can be a stew base.
- `Liquids.RegisterShared(number, spec)` makes a real liquid under a number from 1 to 65535 that no other mod uses. The server and every player need CookingLib and your mod, and both sides must register it.
- `Liquids.Fill(container, id, amount)` pours a liquid into an empty container or one holding the same liquid. Server only.

### Stew recipes

`StewRecipes.Register(spec)` adds a recipe. It borrows the look and name of a vanilla stew (`Look`), takes any registered or vanilla liquids as its base, and can change the stew's colour (`Tint`) or swap the finished stew for a liquid (`ResultLiquid`). Your recipes never show up on recipe pages.

### Infusions

`Infusions.Register(spec)` makes an item dropped into a liquid turn it into another liquid, for example coal into water makes black dye.

### Names

Specs use the game's own names. Run `cookinglib dump` in the server console to write every name to `UserData\CookingLib-dump.txt` and copy them from there. `cookinglib status` lists what registered and why anything was refused.

### Rules

- Ids are written as `author.thing`.
- Colours are 0 to 1 per channel.
- Effect strengths are minus 100 to 100. Players see at most 100, the server applies the full value.
- A stew shows at most 3 extra effects.
- The usual pot checks recipes at exactly 4 ingredients, so give a recipe 4 slots.
- `BaseLiquids` and `InputLiquids` start as Water. Assign a new list to replace it.
- Registering never throws. Check `Error` on what it returns, or read the log.

## Building

Run `dotnet build`. The game path defaults to `C:\Games\Alta\A Township Tale`, pass `-p:GamePath="your path"` to use another one. `python tools/check_style.py` checks the code style.

## License

MIT
