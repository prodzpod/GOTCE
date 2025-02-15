﻿using BepInEx.Configuration;
using HarmonyLib;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GOTCE.Items.VoidYellow
{
    public class CorruptsAllShatterspleens : ItemBase<CorruptsAllShatterspleens>
    {
        public override string ConfigName => "Corrupts all Shatterspleens";

        public override string ItemName => "<style=cIsVoid>Corrupts all Shatterspleens.</style>";

        public override string ItemLangTokenName => "GOTCE_CorruptsAllShatterspleens";

        public override string ItemPickupDesc => "<style=cIsVoid>Corrupts all Shatterspleens.</style>";

        public override string ItemFullDescription => "Gain <style=cIsDamage>5% critical chance</style>. Critical strikes <style=cIsDamage>collapse</style> enemies 1 <style=cStack>(+1 per stack)</style> times for <style=cIsDamage>400%</style> base damage. <style=cIsVoid>Corrupts all Shatterspleens.</style>";

        public override string ItemLore => "<style=cIsVoid>Corrupts all Shatterspleens.</style>";

        public override ItemTier Tier => ItemTier.VoidBoss;

        public override Enum[] ItemTags => new Enum[] { ItemTag.Damage, GOTCETags.Crit };

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => Main.SecondaryAssets.LoadAsset<Sprite>("Assets/Icons/Items/CorruptsAllShatterspleens.png");

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
            RecalculateStatsAPI.GetStatCoefficients += Crit;
            On.RoR2.GlobalEventManager.ServerDamageDealt += NEEDLETICK;
            On.RoR2.Items.ContagiousItemManager.Init += Sussy;
        }

        public void Crit(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body && body.inventory && body.inventory.GetItemCount(ItemDef) > 0)
            {
                args.critAdd += 5f;
            }
        }

        public void NEEDLETICK(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport report)
        {
            orig(report);
            if (report.attacker && report.attackerBody)
            {
                if (report.attackerBody.inventory)
                {
                    int count = report.attackerBody.inventory.GetItemCount(ItemDef);
                    if (count > 0 && report.damageInfo.crit)
                    {
                        int stacks = report.attackerBody.inventory.GetItemCount(ItemDef);
                        if (report.victim && report.victimBody)
                        {
                            float duration = DotController.GetDotDef(DotController.DotIndex.Fracture).interval;
                            report.victimBody.AddTimedBuff(DLC1Content.Buffs.Fracture, duration, stacks);
                            for (int i = 0; i < stacks; i++)
                            {
                                DotController.InflictDot(report.victim.gameObject, report.attacker, DotController.DotIndex.Fracture, duration, 1);
                            }
                        }
                    }
                }
            }
        }

        private void Sussy(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            ItemDef.Pair transformation = new ItemDef.Pair()
            {
                itemDef1 = RoR2Content.Items.BleedOnHitAndExplode,
                itemDef2 = this.ItemDef
            };
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(transformation);
            orig();
        }
    }
}