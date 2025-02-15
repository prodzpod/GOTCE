using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.Skills;

namespace GOTCE.Equipment.EliteEquipment
{
    class BackupMag : EliteEquipmentBase<BackupMag>
    {
        public override string EliteEquipmentName => "Secret Compartment";

        public override string EliteAffixToken => "BACKUP";

        public override string EliteEquipmentPickupDesc => "Become an aspect of backup.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";
        public override Texture2D EliteRampTexture => Main.SecondaryAssets.LoadAsset<Texture2D>("Assets/Prefabs/EliteOverlays/Backup.png");

        public override string EliteModifier => "Compartmentalized";

        public override GameObject EliteEquipmentModel => Main.SecondaryAssets.LoadAsset<GameObject>("Assets/Prefabs/Equipment/Backup/BackupAspect.prefab");

        public override Sprite EliteEquipmentIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Equipment/BackupAspect.png");
        public override Sprite EliteBuffIcon => Main.SecondaryAssets.LoadAsset<Sprite>("Assets/Icons/Buffs/BackupAffixIcon.png");
        public override float DamageMultiplier => 2f;
        public override float HealthMultiplier => 4f;
        public override CombatDirector.EliteTierDef[] CanAppearInEliteTiers => EliteAPI.GetCombatDirectorEliteTiers().Where(x => x.eliteTypes.Contains(Addressables.LoadAssetAsync<EliteDef>("RoR2/Base/EliteFire/edFire.asset").WaitForCompletion())).ToArray();

        public override void Init(ConfigFile config)
        {
            Backuped.Awake();
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {

        }

        private void CreateEliteTiers()
        {
        }


        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += Backup;
            GlobalEventManager.onServerDamageDealt += OnDamage;
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public void OnDamage(DamageReport report) {
            if (report.attacker && report.attackerBody && report.victim && report.victimBody) {
                if (report.attackerBody.equipmentSlot) {
                    CharacterBody body = report.attackerBody;
                    CharacterBody victim = report.victimBody;
                    if (body.HasBuff(EliteBuffDef)) {
                        float duration = 5.0f * report.damageInfo.procCoefficient;
                        victim.AddTimedBuff(Backuped.buff, duration, 1);
                    }
                }
            }
        }

        public void Backup(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args) {
            if (body.HasBuff(EliteBuffDef)) {
                if (body.skillLocator) {
                    args.secondaryCooldownMultAdd -= 0.5f;
                    if (body.skillLocator && body.skillLocator.secondary) {
                        body.skillLocator.secondary.SetBonusStockFromBody(3 + body.skillLocator.secondary.bonusStockFromBody);
                    }
                }
            }

            if (body.HasBuff(Backuped.buff) && body.skillLocator && body.skillLocator.secondary) {
                SkillDef lockedDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/CaptainSkillDisconnected.asset").WaitForCompletion();
                body.skillLocator.secondary.SetSkillOverride(body, lockedDef, GenericSkill.SkillOverridePriority.Replacement);
            }
            else if (body.skillLocator && body.skillLocator.secondary) {
                SkillDef lockedDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/CaptainSkillDisconnected.asset").WaitForCompletion();
                body.skillLocator.secondary.UnsetSkillOverride(body, lockedDef, GenericSkill.SkillOverridePriority.Replacement);
            }
        }
    }

    public class Backuped {
        public static BuffDef buff;

        public static void Awake() {
            buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.canStack = false;
            buff.isDebuff = true;
            buff.name = "Backuped";
            buff.iconSprite = Main.SecondaryAssets.LoadAsset<Sprite>("Assets/Icons/Buffs/BackupAffixIcon.png");

            R2API.ContentAddition.AddBuffDef(buff);
        }
    }
}