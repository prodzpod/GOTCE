﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GOTCE.Items.Yellow
{
    public class RightRingFingerOfProvidence : ItemBase<RightRingFingerOfProvidence>
    {
        public override string ConfigName => "Right Ring Finger of Providence";

        public override string ItemName => "Right Ring Finger of Providence";

        public override string ItemLangTokenName => "GOTCE_ProvidenceFinger";

        public override string ItemPickupDesc => "Double ALL of your stats.";

        public override string ItemFullDescription => "Increase your damage, attack speed, jump height, movement speed, health, luck, crit chances, AOE effect, cooldowns, shield, regen, and size by 100% (+100% per stack).";

        public override string ItemLore => "Fortnite sussy balls idk";

        public override ItemTier Tier => ItemTier.Boss;

        public override Enum[] ItemTags => new Enum[] { ItemTag.BrotherBlacklist, ItemTag.Damage, ItemTag.Utility, ItemTag.Healing, GOTCETags.Crit, GOTCETags.Cracked };

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override void Init(ConfigFile config)
        {
            base.Init(config);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict(null);
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                var stack = sender.inventory.GetItemCount(Instance.ItemDef);
                if (stack > 0)
                {
                    args.attackSpeedMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.critDamageMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.jumpPowerMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.damageMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.healthMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.moveSpeedMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.regenMultAdd += sender.regen * stack;
                    args.cooldownMultAdd += Mathf.Pow(2f, stack) - 1f;
                    args.shieldMultAdd += Mathf.Pow(2f, stack) - 1f;
                }
            }
        }
    }
}