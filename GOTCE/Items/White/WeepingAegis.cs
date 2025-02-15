﻿using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GOTCE.Components;
using BepInEx.Configuration;
using HarmonyLib;

namespace GOTCE.Items.White
{
    public class WeepingAegis : ItemBase<WeepingAegis>
    {
        public override string ConfigName => "Weeping Aegis";

        public override string ItemName => "Weeping Aegis";

        public override string ItemLangTokenName => "GOTCE_WeepingAegis";

        public override string ItemPickupDesc => "Killing a bleeding enemy permanently increases your shield.";

        public override string ItemFullDescription => "On killing a <style=cIsDamage>bleeding</style> enemy, gain <style=cIsUtility>3</style> <style=cStack>(+3 per stack)</style> <style=cIsUtility>shield</style> permanently.";

        public override string ItemLore => "cry about it";

        public override ItemTier Tier => ItemTier.Tier1;
        public override Enum[] ItemTags => new Enum[] { ItemTag.Utility, ItemTag.OnKillEffect };

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Item/WeepingAegis.png");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict(null);
        }

        public override void Init(ConfigFile config)
        {
            base.Init(config);
        }

        public int shieldBoost = 0;

        public override void Hooks()
        {
            On.RoR2.Run.Start += Run_Start;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            shieldBoost = 0;
            orig(self);
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, RoR2.GlobalEventManager self, RoR2.DamageReport damageReport)
        {
            orig(self, damageReport);
            if (damageReport.attackerBody == null)
            {
                return;
            }
            var stack = damageReport.attackerBody.inventory.GetItemCount(Instance.ItemDef);
            if (stack > 0 && damageReport.victimBody && (damageReport.victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || damageReport.victimBody.HasBuff(RoR2Content.Buffs.SuperBleed) || damageReport.dotType == DotController.DotIndex.Bleed || damageReport.dotType == DotController.DotIndex.SuperBleed || damageReport.damageInfo.dotIndex == DotController.DotIndex.Bleed || damageReport.damageInfo.dotIndex == DotController.DotIndex.SuperBleed))
            {
                shieldBoost += 15 + 14 * (stack - 1);
                damageReport.attackerBody.RecalculateStats();
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (self.inventory)
            {
                var stack = self.inventory.GetItemCount(Instance.ItemDef);
                if (stack > 0)
                {
                    self.baseMaxHealth += shieldBoost;
                    orig(self);
                    self.baseMaxHealth -= shieldBoost;
                    return;
                }
            }
            orig(self);
        }
    }
}