﻿using BepInEx;
using GOTCE.Artifact;
using GOTCE.Enemies.Changes;

//using GOTCE.Enemies.Normal_Enemies;
using GOTCE.Equipment;
using GOTCE.Equipment.EliteEquipment;
using GOTCE.Items;
using GOTCE.Misc;
using Mono.Cecil;
using MonoMod.Cil;
using GOTCE.Tiers;
using Mono.Cecil.Cil;
using GOTCE.Buffs;
using GOTCE.Stages;
using R2API;
using R2API.Networking;
using GOTCE.Achievements;
using R2API.Utils;
using System.Text.RegularExpressions;
using RoR2;
using System.Runtime.CompilerServices;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SearchableAttribute = HG.Reflection.SearchableAttribute;
using GOTCE.Skills;
using GOTCE.Interactables;
using GOTCE.Enemies;
using RoR2.ExpansionManagement;
using Object = UnityEngine.Object;
using GOTCE.Enemies.Standard;
using GOTCE.Enemies.Minibosses;
using GOTCE.Enemies.Bosses;
using MonoMod.RuntimeDetour;
using GOTCE.Based;
using GOTCE.Survivors;
using BetterUI;
using UnityEngine.SceneManagement;
using R2API.ContentManagement;
using RoR2.UI.MainMenu;
using BepInEx.Configuration;

//using NemesisSlab;

[assembly: SearchableAttribute.OptIn]

namespace GOTCE
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(DamageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ItemAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(EliteAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DirectorAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(NetworkingAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(SoundAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)] // soft dependency for compat
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.TheBestAssociatedLargelyLudicrousSillyheadGroup.GOTCE";
        public const string ModName = "Gamers of the Cracked Emoji";
        public const string ModVer = "1.3.2";

        public static AssetBundle MainAssets;
        public static AssetBundle SecondaryAssets;
        public static AssetBundle GOTCEModels;
        public static AssetBundle SceneBundle;
        public static uint GOTCESounds;
        public List<ArtifactBase> Artifacts = new();
        public List<ItemBase> Items = new();
        public List<EquipmentBase> Equipments = new();
        public List<EliteEquipmentBase> EliteEquipments = new();

        public static Dictionary<ArtifactBase, bool> ArtifactStatusDictionary = new();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new();
        public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new();

        public static ExpansionDef GOTCEExpansionDef;
        public static ExpansionDef SOTVExpansionDef;
        public static GameObject GOTCERunBehavior;

        //Provides a direct access to this plugin's logger for use in any of your other classes.
        public static BepInEx.Logging.ManualLogSource ModLogger;

        private static Shader cloudRemap;
        private static Shader standard;
        private static Shader terrain;
        public static bool HasPatched = false;
        public static ConfigFile config;

        private void Awake()
        {
            MainAssets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("GOTCE.dll", "macterabrundle"));
            SecondaryAssets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("GOTCE.dll", "secondarybundle"));
            GOTCEModels = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("GOTCE.dll", "gotcemodels"));
            SceneBundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("GOTCE.dll", "scenebundle"));
            GOTCESounds = SoundAPI.SoundBanks.Add(Assembly.GetExecutingAssembly().Location.Replace("GOTCE.dll", "GOTCE.bnk"));
            ModLogger = Logger;
            SOTVExpansionDef = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();

            config = Config;

            cloudRemap = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGCloudRemap.shader").WaitForCompletion();
            standard = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion();
            terrain = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGTriplanarTerrainBlend.shader").WaitForCompletion();

            RoR2Application.onLoad += () => { Util.PlaySound("The", RoR2Application.instance.gameObject); }; // the

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI") && Config.Bind<bool>("Compatibility", "BetterUI - Stats Display", true, "Adds the GOTCE stats to the BetterUI stats display.").Value)
            {
                UICompat.AddBetterUICompat();
            }

            On.RoR2.PurchaseInteraction.Awake += (orig, self) =>
            {
                orig(self);
                if (SceneManager.GetActiveScene().name == "bazaar" && self.GetComponent<RoR2.EntityLogic.Counter>())
                {
                    MonoBehaviour[] coms = self.GetComponentsInChildren<MonoBehaviour>();
                    for (int i = 0; i < coms.Length; i++)
                    {
                        coms[i].enabled = false;
                    }

                    MeshRenderer[] renderers = self.GetComponentsInChildren<MeshRenderer>();
                    ParticleSystemRenderer[] parts = self.GetComponentsInChildren<ParticleSystemRenderer>();

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].enabled = false;
                    }

                    for (int i = 0; i < parts.Length; i++)
                    {
                        parts[i].enabled = false;
                    }
                }
            };

            HooksAttributeLogic.Scan();

            HooksAttributeLogic.CallAttributeMethods(RunAfter.Start);

            // create custom itemtags and flags and things idk
            Flags.Initialize();

            // please just fucking use hopoo shaders AAAAAA
            // https://drive.google.com/drive/folders/1ndCC4TiN06nVC4X_3HaZjFa5sN07Y14S

            var mat1 = MainAssets.LoadAllAssets<Material>();
            foreach (Material material in mat1)
            {
                switch (material.shader.name)
                {
                    case "StubbedShader/fx/hgcloudremap":
                        material.shader = cloudRemap;
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        material.shader = standard;
                        break;

                    case "StubbedShader/deferred/hgtriplanarterrainblend":
                        material.shader = terrain;
                        break;
                }
            }

            var mat2 = SecondaryAssets.LoadAllAssets<Material>();
            foreach (Material material in mat2)
            {
                switch (material.shader.name)
                {
                    case "StubbedShader/fx/hgcloudremap":
                        material.shader = cloudRemap;
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        material.shader = standard;
                        break;

                    case "StubbedShader/deferred/hgtriplanarterrainblend":
                        material.shader = terrain;
                        break;
                }
            }

            var mat3 = GOTCEModels.LoadAllAssets<Material>();
            foreach (Material material in mat3)
            {
                switch (material.shader.name)
                {
                    case "StubbedShader/fx/hgcloudremap":
                        material.shader = cloudRemap;
                        break;

                    case "StubbedShader/deferred/hgstandard":
                        material.shader = standard;
                        break;

                    case "StubbedShader/deferred/hgtriplanarterrainblend":
                        material.shader = terrain;
                        break;
                }
            }

            /* if (Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI")) {
                ItemSorting.tierMap.Add(LunarVoid.Instance.TierEnum, 3);
            } */

            // Don't know how to create/use an asset bundle, or don't have a unity project set up?
            // Look here for info on how to set these up: https://github.com/KomradeSpectre/AetheriumMod/blob/rewrite-master/Tutorials/Item%20Mod%20Creation.md#unity-project
            // (This is a bit old now, but the information on setting the unity asset bFream("GOTCE.macterabrundle"))
            //{
            //MainAssets = AssetBundle.LoadFromStream(stream);
            //}

            //This section automatically scans the project for all artifacts
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                //ModLogger.LogInfo(artifact.ArtifactDescription);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }

            CriticalTypes.Hooks();

            // grab tiers and add them
            var Tiers = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(TierBase)));

            foreach (var tier in Tiers)
            {
                TierBase Tier = (TierBase)Activator.CreateInstance(tier);
                Tier.Awake();
            }

            //This section automatically scans the project for all items
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                // Debug.Log(item.ConfigName);
                if (ValidateItem(item, Items)) // remove right side after release
                {
                    item.Init(Config);
                }
            }

            [SystemInitializer(dependencies: typeof(ItemCatalog))]
            void callitems()
            {
                HooksAttributeLogic.CallAttributeMethods(RunAfter.Items);
            }

            //this section automatically scans the project for all equipment
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                }
            }

            //this section automatically scans the project for all stages
            var StageTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(StageBase)));

            foreach (var stageType in StageTypes)
            {
                StageBase stage = (StageBase)System.Activator.CreateInstance(stageType);
                stage.Create(Config);
            } 

            //this section automatically scans the project for all elite equipment
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)System.Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);
                }
            }

            // achievements
            var Achievements = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(AchievementBase)));
            foreach (var unlock in Achievements)
            {
                AchievementBase achiev = (AchievementBase)Activator.CreateInstance(unlock);
                achiev.Create(Config);
            }

            //this section automatically scans the project for all equipment
            var SkillTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SkillBase)));

            foreach (var skillType in SkillTypes)
            {
                SkillBase skill = (SkillBase)System.Activator.CreateInstance(skillType);
                skill.Create();
            }

            AltSkills.AddAlts();

            [SystemInitializer(dependencies: typeof(RoR2.Skills.SkillCatalog))]
            void callskills()
            {
                HooksAttributeLogic.CallAttributeMethods(RunAfter.Skills);
            }

            Itsgup.OhTheMisery();
            // LivingSuppressiveFire.Create();
            // IonSurger.Create(); // ION SURGER IS BROKEN
            Itsgup.SoMyMainGoalIsToBlowUp();
            GOTCE.Based.asfk23A.Df23__23aFKLNQ();
            Based.SuppressiveNader.Hook();
            Based.Logbook.RunHooks();
            Fragile.Hook();

            var enemyTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EnemyBase)));

            foreach (var enemyType in enemyTypes)
            {
                // Debug.Log("Woolie");
                EnemyBase enemy = (EnemyBase)System.Activator.CreateInstance(enemyType);
                // Debug.Log(item.ConfigName);
                if (ValidateEnemy(enemy))
                {
                    enemy.Create();
                }
            }

            [SystemInitializer(dependencies: typeof(BodyCatalog))]
            void callenemies()
            {
                HooksAttributeLogic.CallAttributeMethods(RunAfter.Enemies);
            }


            var survivorTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SurvivorBase)));

            foreach (var survivorType in survivorTypes)
            {
                // Debug.Log("Woolie");
                SurvivorBase survivor = (SurvivorBase)System.Activator.CreateInstance(survivorType);
                // Debug.Log(item.ConfigName);
                survivor.Create();
            }

            var buffTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BuffBase)));

            foreach (var buffType in buffTypes)
            {
                // Debug.Log("Woolie");
                BuffBase buff = (BuffBase)System.Activator.CreateInstance(buffType);
                // Debug.Log(item.ConfigName);
                buff.CreateBuff(Config);
            }

            MonoMod.RuntimeDetour.Hook aimHook = new MonoMod.RuntimeDetour.Hook(
                typeof(InputBankTest).GetProperty("aimOrigin", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetGetMethod(),
                typeof(LivingSuppressiveFire).GetMethod("InputBankTest_aimOrigin_Get", System.Reflection.BindingFlags.Public | BindingFlags.Static)
            );

            Gamemodes.Crackclipse.GameMode.Create();

            //CreateExpansion();
            // On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
            // local multiplayer hook
            // run modded ror2 twice, create a multiplayer lobby in one, then do connect localhost:7777 in the other instance
        }

        [SystemInitializer(typeof(ItemCatalog))]
        public static void PostItemCat() {
            Woolie.Initialize();
            WarCrimes.Hooks();
            AOEffect.Hooks();

            Main.ModLogger.LogInfo("Initializing equipments.");

            var interactableTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(InteractableBase)));

            foreach (var interactableType in interactableTypes)
            {
                InteractableBase inter = (InteractableBase)System.Activator.CreateInstance(interactableType);
                // inter.Create();

                if (ValidateInteractable(inter))
                {
                    inter.Create();
                }
            }
                
            HooksAttributeLogic.CallAttributeMethods(RunAfter.Items);
            HooksAttributeLogic.CallAttributeMethods(RunAfter.Misc);
        }

        /// <summary>
        /// A helper to easily set up and initialize an artifact from your artifact classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="artifact">A new instance of an ArtifactBase class."</param>
        /// <param name="artifactList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;

            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an item from your item classes if the user has it enabled in their configuration files.
        /// <para>Additionally, it generates a configuration for each item to allow blacklisting it from AI.</para>
        /// </summary>
        /// <param name="item">A new instance of an ItemBase class."</param>
        /// <param name="itemList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateItem(ItemBase item, List<ItemBase> itemList, bool faulty = false)
        {
            if (item.Tier == ItemTier.NoTier)
            {
                return true;
            }

            var enabled1 = Config.Bind<bool>("Item: " + item.ConfigName, "Enable Item?", true, "Should this item appear in runs?").Value;

            var aiBlacklist = Config.Bind<bool>("Item: " + item.ConfigName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            if (enabled1)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
            }
            // Debug.Log(item.ConfigName + " : " + enabled1);
            return enabled1;
        }

        /// <summary>
        /// A helper to easily set up and initialize an enemy from your enemy classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="enemy">A new instance of an EnemyBase class."</param>
        public bool ValidateEnemy(EnemyBase enemy)
        {
            if (enemy.CloneName == "Explosive Decoy")
            {
                return true;
            }
            bool enabled = Config.Bind<bool>("Enemy: " + enemy.CloneName, "Enable Enemy?", true, "Should this enemy appear in runs?").Value;
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an equipment from your equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="equipment">A new instance of an EquipmentBase class."</param>
        /// <param name="equipmentList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        public static bool ValidateInteractable(InteractableBase i)
        {
            if (Main.config.Bind<bool>("Interactable: " + i.Name, "Enable Interactable?", true, "Should this interactable appear in runs?").Value)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// A helper to easily set up and initialize an elite equipment from your elite equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="eliteEquipment">A new instance of an EliteEquipmentBase class.</param>
        /// <param name="eliteEquipmentList">The list you would like to add this to if it passes the config check.</param>
        /// <returns></returns>
        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + eliteEquipment.EliteEquipmentName, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.").Value;

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public void CreateExpansion()
        {
            var sotv = LegacyResourcesAPI.Load<ExpansionDef>("ExpansionDefs/DLC1");

            GOTCEExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();
            //var what = Addressables.LoadAssetAsync<GameObject>("12bf89dabb4bb914382a0e31546446cc").WaitForCompletion();
            GOTCERunBehavior = PrefabAPI.InstantiateClone(gameObject, "GOTCERunBehavior", true);
            DestroyImmediate(GOTCERunBehavior.GetComponent<GlobalDeathRewards>());
            var expansionRequirement = GOTCERunBehavior.GetComponent<ExpansionRequirementComponent>();
            expansionRequirement.requiredExpansion = GOTCEExpansionDef;
            /*
            GOTCERunBehavior.AddComponent<GOTCEVisuals>();
            GOTCERunBehavior.AddComponent<GOTCEBuffs>();
            GOTCERunBehavior.AddComponent<GOTCEEliteRamps>();
            GOTCERunBehavior.AddComponent<GOTCEDamageColors>();
            */
            PrefabAPI.RegisterNetworkPrefab(GOTCERunBehavior);
            // SpikestripContentBase.networkedObjectContent.Add(GOTCERunBehavior);
            GOTCEExpansionDef.name = "GOTCECONTENT_EXPANSION";
            GOTCEExpansionDef.nameToken = "GOTCECONTENT_EXPANSION_NAME";
            GOTCEExpansionDef.descriptionToken = "GOTCECONTENT_EXPANSION_DESCRIPTION";
            GOTCEExpansionDef.iconSprite = MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Item/NEA.png");
            GOTCEExpansionDef.disabledIconSprite = sotv.disabledIconSprite;
            GOTCEExpansionDef.requiredEntitlement = sotv.requiredEntitlement;
            GOTCEExpansionDef.runBehaviorPrefab = GOTCERunBehavior;
            LanguageAPI.Add(GOTCEExpansionDef.nameToken, "Gamers of The Cracked Emoji");
            LanguageAPI.Add(GOTCEExpansionDef.descriptionToken, "Adds content from the 'GOTCE' mod to the game.");
        }
    }

    public class UICompat
    {
        private static List<string> cachedNormalText = null;
        private static List<string> cachedAltText = null;

        public delegate void orig_onStart();

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddBetterUICompat()
        {
            // custom stats in the display
            Func<CharacterBody, string> stage = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().stageCritChance.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> war = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        WarCrime crime = body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().mostRecentlyCommitedWarCrime;
                        string name = "N/A";
                        WarCrimes.CrimeToName.TryGetValue(crime, out name);
                        return name;
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> sprint = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().sprintCritChance.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> fov = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().fovCritChance.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> rotation = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().rotationCritChance.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> death = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().deathCritChance.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> aoe = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().aoeEffect.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            Func<CharacterBody, string> revive = (CharacterBody body) =>
            {
                if (body.masterObject)
                {
                    if (body.masterObject.GetComponent<Components.GOTCE_StatsComponent>())
                    {
                        return body.masterObject.GetComponent<Components.GOTCE_StatsComponent>().reviveChance.ToString();
                    }
                    else
                    {
                        return "N/A";
                    }
                }
                else
                {
                    return "N/A";
                }
            };

            StatsDisplay.AddStatsDisplay("$stage", stage);
            StatsDisplay.AddStatsDisplay("$sprint", sprint);
            StatsDisplay.AddStatsDisplay("$fov", fov);
            StatsDisplay.AddStatsDisplay("$war", war);
            StatsDisplay.AddStatsDisplay("$aoe", aoe);
            StatsDisplay.AddStatsDisplay("$rotation", rotation);
            StatsDisplay.AddStatsDisplay("$death", death);
            // StatsDisplay.AddStatsDisplay("$revive", revive);

            /* Hook statsHook = new Hook(
                typeof(BetterUI.StatsDisplay).GetMethod("onStart", (BindingFlags)(-1)),
                typeof(UICompat).GetMethod(nameof(onStart), (BindingFlags)(-1))
            ); */

            On.RoR2.UI.HUD.Awake += (orig, self) =>
            {
                orig(self);
                List<string> normalText;
                List<string> altText;
                if (cachedNormalText == null)
                {
                    normalText = typeof(StatsDisplay).GetFieldValue<string[]>("normalText").ToList();
                    string[] tmp = new string[normalText.Count]; ;
                    normalText.CopyTo(tmp);
                    cachedNormalText = tmp.ToList();
                }
                else
                {
                    string[] tmp = new string[cachedNormalText.Count];
                    cachedNormalText.CopyTo(tmp);
                    normalText = tmp.ToList();
                }

                if (cachedAltText == null)
                {
                    altText = typeof(StatsDisplay).GetFieldValue<string[]>("altText").ToList();
                    string[] tmp = new string[altText.Count];
                    altText.CopyTo(tmp);
                    cachedAltText = tmp.ToList();
                }
                else
                {
                    string[] tmp = new string[cachedAltText.Count]; ;
                    cachedAltText.CopyTo(tmp);
                    altText = tmp.ToList();
                }
                normalText.RemoveAt(normalText.Count - 1);
                normalText.Add("\nStage Crit: ");
                normalText.Add("$stage");
                // normalText.Add("%");
                normalText.Add("%\nFOV Crit: ");
                normalText.Add("$fov");
                // normalText.Add("%");
                normalText.Add("%\nSprint Crit: ");
                normalText.Add("$sprint");
                normalText.Add("%\nRecent War Crime: ");
                normalText.Add("$war");
                normalText.Add("\nDeath Crit: ");
                normalText.Add("$death");
                normalText.Add("%\nRotation Crit: ");
                normalText.Add("$rotation");
                normalText.Add("%\nAoE Effect: +");
                normalText.Add("$aoe");

                /* string[] guh = normalText.ToArray();
                for (int i = 0; i < guh.Length; i++) {
                    Debug.Log(normalText[i]);
                    if (i % 2 == 0) {
                        Debug.Log("% 2 is true");
                    }
                } */
                altText.RemoveAt(altText.Count - 1);
                altText.Add("\nStage Crit: ");
                altText.Add("$stage");
                // altText.Add("%");
                altText.Add("%\nFOV Crit: ");
                altText.Add("$fov");
                // altText.Add("%");
                altText.Add("%\nSprint Crit: ");
                altText.Add("$sprint");
                // altText.Add("%");
                altText.Add("%\nRecent War Crime: ");
                altText.Add("$war");
                altText.Add("\nDeath Crit: ");
                altText.Add("$death");
                altText.Add("%\nRotation Crit: ");
                altText.Add("$rotation");
                altText.Add("%\nAoE Effect: +");
                altText.Add("$aoe");
                // altText.Add("Revive Chance: ");
                // altText.Add("$revive");
                // altText.Add("%");

                typeof(StatsDisplay).SetFieldValue<string[]>("normalText", normalText.ToArray());
                typeof(StatsDisplay).SetFieldValue<string[]>("altText", altText.ToArray());
            };
        }

        /* public static void onStart(orig_onStart orig) {
            orig();

            Debug.Log(normalText);
            Debug.Log("====== alt =====");
            Debug.Log("");
        } */
    }

    /* public class SlabAntiCompat {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Eradicate() {
            MethodInfo[] methods = typeof(NemesisSlab.NemesisMain).GetMethods();
            List<string> methodNames = new();

            foreach (MethodInfo method in methods) {
                Debug.Log(method.Name);
                if (method.ReturnType == typeof(void)) {
                    methodNames.Add(method.Name);
                }
            }

            foreach (string name in methodNames) {
                if (!name.ToLower().Contains("get") && !name.ToLower().Contains("set")) {
                    Main.ModLogger.LogDebug("Patching method: " + name);
                    ILHook hook = new ILHook(
                        typeof(NemesisMain).GetMethod(name, (BindingFlags)(-1)),
                        new ILContext.Manipulator(Destroy)
                    );
                }
            }
        }

        public static void Destroy(ILContext il) {
            ILCursor c = new ILCursor(il);
            c.Index = 0;
            c.Emit(OpCodes.Ret);
        }
    } */
}