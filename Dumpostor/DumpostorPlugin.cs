using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Dumpostor.Dumpers;
using HarmonyLib;
using InnerNet;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dumpostor
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    public class DumpostorPlugin : BasePlugin
    {
        public const string Id = "pl.js6pak.dumpostor";

        public Harmony Harmony { get; } = new Harmony(Id);

        public ConfigEntry<string> DumpedPath { get; private set; }

        public override void Load()
        {
            DumpedPath = Config.Bind("Settings", "DumpedPath", string.Empty);

            Harmony.PatchAll();

            foreach (HatManager manager in GameObject.FindObjectsOfType<HatManager>())
            {
                Log.LogInfo($"Number of hats: {manager.AllHats.Count}");
            }
            
            if (string.IsNullOrEmpty(DumpedPath.Value) || !Directory.Exists(DumpedPath.Value))
            {
                Log.LogError("DumpedPath is empty or is not a directory");
                Application.Quit();
                return;
            }

            Log.LogDebug("Dumping to " + DumpedPath.Value);

            EntrypointPatch.Initialized += () =>
            {
                var dumpers = new IDumper[]
                {
                    new EnumDumper<StringNames>(),
                    new EnumDumper<SystemTypes>(),
                    new EnumDumper<TaskTypes>(),
                    new EnumDumper<GameKeywords>(),
                    new EnumDumper<DisconnectReasons>(),
                    new HatDumper(),
                    new ColorDumper(),
                    new TranslationsDumper()
                };

                foreach (var dumper in dumpers)
                {
                    Log.LogInfo("Dumping " + dumper);

                    var dump = dumper.Dump();
                    if (dump != null)
                    {
                        File.WriteAllText(Path.Combine(DumpedPath.Value, dumper.FileName), dump);
                    }
                }

                var mapDumpers = new IMapDumper[]
                {
                    new TaskDumper(),
                    new VentDumper()
                };

                foreach (var mapType in (MapTypes[]) Enum.GetValues(typeof(MapTypes)))
                {
                    var shipPrefab = AmongUsClient.Instance.ShipPrefabs[(int)mapType];
                    DumpCoroutine(mapType, shipPrefab, mapDumpers);
                }
            };
        }

        public void DumpCoroutine(MapTypes mapType, AssetReference assetReference, IEnumerable<IMapDumper> dumpers)
        {
            var shipPrefab = assetReference.Instantiate();

            var shipStatus = shipPrefab.Result.GetComponent<ShipStatus>();

            var directory = Path.Combine(DumpedPath.Value, mapType.ToString());
            Directory.CreateDirectory(directory);

            Log.LogInfo("Dumping map " + mapType);

            foreach (var dumper in dumpers)
            {
                Log.LogInfo("Dumping " + dumper);
                File.WriteAllText(Path.Combine(directory, dumper.FileName), dumper.Dump(mapType, shipStatus));
            }

            assetReference.ReleaseInstance(shipPrefab.Result);
        }

        [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.Initialize))]
        private static class EntrypointPatch
        {
            public static event Action Initialized;

            public static void Postfix(AmongUsClient __instance)
            {
                Initialized?.Invoke();
            }
        }
    }
}
