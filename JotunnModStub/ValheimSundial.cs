// ValheimSundial
// A Valheim mod that adds a Sundial block
// 
// File:    ValheimSundial.cs
// Project: ValheimSundial

using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;

namespace ValheimSundial
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ValheimSundial : BaseUnityPlugin
    {
        public const string PluginGUID = "com.donchad.ValheimSundial";
        public const string PluginName = "ValheimSundial";
        public const string PluginVersion = "0.0.2";

        public static ConfigEntry<bool> isDebug;

        private AssetBundle sundial_prefab_bundle;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
        public static CustomLocalization Localization;

        private void Awake()
        {
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");

            // Jotunn comes with MonoMod Detours enabled for hooking Valheim's code
            // https://github.com/MonoMod/MonoMod
            On.FejdStartup.Awake += FejdStartup_Awake;

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("ValheimSundial has awoken");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html

            PrefabManager.OnVanillaPrefabsAvailable += CreateKitBashDial;
            AddLocalizations();
        }

        private void FejdStartup_Awake(On.FejdStartup.orig_Awake orig, FejdStartup self)
        {
            // This code runs before Valheim's FejdStartup.Awake
            Jotunn.Logger.LogInfo("FejdStartup is going to awake");

            // Call this method so the original game method is invoked
            orig(self);

            // This code runs after Valheim's FejdStartup.Awake
            Jotunn.Logger.LogInfo("FejdStartup has awoken");
        }


        private void AddLocalizations()
        {
            Localization = new CustomLocalization();
            LocalizationManager.Instance.AddLocalization(Localization);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                { "sundial_collider_name", "Stone Sundial" },
                { "sundial_collider_description", "You smashed two rocks together and can now tell the time" },
                { "prop_sundial", "The time here" },
            });

        }

        private void CreateKitBashDial()
        {
            sundial_prefab_bundle = AssetUtils.LoadAssetBundleFromResources("sundial_collider", typeof(ValheimSundial).Assembly);
            try
            {
                KitbashObject kbo = KitbashManager.Instance.AddKitbash(sundial_prefab_bundle.LoadAsset<GameObject>("sundial_collider"), new KitbashConfig
                {
                    Layer = "piece",
                    KitbashSources = new List<KitbashSourceConfig>
                {
                    new KitbashSourceConfig
                    {
                        Name = "sundial_base",
                        SourcePrefab = "SharpeningStone",
                        SourcePath = "model",
                        Position = new Vector3(0f, -1f, 0f),   // width, height, width.
                        Rotation = Quaternion.Euler(0, 0, 90f),
                        Scale = new Vector3(3f, 1f, 1f),       // height, width, width.
                        Materials = new string[]{"stone_large"}
                    },

                    new KitbashSourceConfig
                    {
                        Name = "sundial_dial",
                        SourcePrefab = "highstone",
                        SourcePath = "high",
                        Position = new Vector3(0f, -0.8f, 0f),
                        Rotation = Quaternion.Euler(0, 0, 75f),
                        Scale = new Vector3(0.06f, 0.09f, 0.06f)
                    }
                }
                });
                CustomPiece piece = new CustomPiece(kbo.Prefab, fixReference: false,
                        new PieceConfig
                        {
                            PieceTable = "Hammer",
                            Requirements = new RequirementConfig[]
                            {
                                new RequirementConfig {Item = "Stone", Amount = 2, Recover = true}
                            }
                        }
                    );
                piece.PiecePrefab.AddComponent<SundialTextHover>();

                // get effects from stone_arch
                var vfx_Place_stone_wall_2x1 = new EffectList.EffectData();
                vfx_Place_stone_wall_2x1.m_prefab = PrefabManager.Instance.GetPrefab("vfx_Place_stone_wall_2x1");
                var sfx_build_hammer_stone = new EffectList.EffectData();
                sfx_build_hammer_stone.m_prefab = PrefabManager.Instance.GetPrefab("sfx_build_hammer_stone");

                piece.Piece.m_placeEffect.m_effectPrefabs = new EffectList.EffectData[]
                {
                    vfx_Place_stone_wall_2x1,
                    sfx_build_hammer_stone
                };


                var vfx_RockDestroyed = new EffectList.EffectData();
                vfx_RockDestroyed.m_prefab = PrefabManager.Instance.GetPrefab("vfx_RockDestroyed");
                var sfx_rock_destroyed = new EffectList.EffectData();
                sfx_rock_destroyed.m_prefab = PrefabManager.Instance.GetPrefab("sfx_rock_destroyed");

                piece.Piece.gameObject.GetComponent<WearNTear>().m_destroyedEffect.m_effectPrefabs = new EffectList.EffectData[]
                {
                    vfx_RockDestroyed,
                    sfx_rock_destroyed
                };

                piece.FixReference = true;
                PieceManager.Instance.AddPiece(piece);
            }
            finally
            {
                PrefabManager.OnVanillaPrefabsAvailable -= CreateKitBashDial;
                sundial_prefab_bundle.Unload(false);
            }
        }
    }
}