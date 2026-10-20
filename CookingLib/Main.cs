using System;
using System.Linq;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CookingLib.Main), "CookingLib", CookingLib.Main.Version, "Shadow")]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace CookingLib
{
    public class Main : MelonMod
    {
        public const string Version = "0.1.1";

        public static event Action PreSetup;
        public static event Action Setup;
        public static event Action PostSetup;

        public static bool IsServer { get; private set; }

        static bool worldUp;

        public override void OnInitializeMelon()
        {
            Log.Init(LoggerInstance);
            IsServer = Application.isBatchMode || Environment.GetCommandLineArgs().Any(arg => arg.Equals("/start_server", StringComparison.OrdinalIgnoreCase));
            Log.Info(IsServer ? "running on a server" : "running on a client");
            if (IsServer)
                ServerPatches.Apply(HarmonyInstance);
        }

        public override void OnLateInitializeMelon()
        {
            Fire(PreSetup, nameof(PreSetup));
            Fire(Setup, nameof(Setup));
            Fire(PostSetup, nameof(PostSetup));
        }

        public override void OnUpdate()
        {
            if (worldUp || !IsServer || NetworkSceneManager.Current == null)
                return;

            worldUp = true;
            StewRecipes.ResolveAll();
            Infusions.ResolveAll();
            CookingLibCommands.Register();
        }

        static void Fire(Action handlers, string name)
        {
            if (handlers == null)
                return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception e)
                {
                    Log.Error(name + " handler " + handler.Method.DeclaringType?.FullName + "." + handler.Method.Name + " failed, the others still run");
                    Log.Error(e.ToString());
                }
            }
        }
    }
}
