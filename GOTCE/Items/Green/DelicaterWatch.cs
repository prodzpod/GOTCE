using R2API;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.Networking;

namespace GOTCE.Items.Green
{
    public class DelicaterWatch : ItemBase<DelicaterWatch>
    {
        public override string ConfigName => "Delicater Watch";

        public override string ItemName => "Delicater Watch";

        public override string ItemLangTokenName => "GOTCE_DelicaterWatch";

        public override string ItemPickupDesc => "Deal increased damage. Breaks upon activating the teleporter.";

        public override string ItemFullDescription => "Gain +100% (+100% per stack) damage. All stacks break upon activating the teleporter.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage, ItemTag.AIBlacklist };

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
            On.RoR2.TeleporterInteraction.OnInteractionBegin += BreakStacks;
            RecalculateStatsAPI.GetStatCoefficients += Damage;
        }

        public void BreakStacks(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            var instances = PlayerCharacterMasterController.instances;
            foreach (PlayerCharacterMasterController playerCharacterMaster in instances)
            {
                if (playerCharacterMaster.master.GetBody().inventory.GetItemCount(ItemDef) > 0)
                {
                    var body = playerCharacterMaster.master.GetBody();
                    body.inventory.GiveItem(GOTCE.Items.NoTier.DelicatestWatch.Instance.ItemDef, playerCharacterMaster.master.GetBody().inventory.GetItemCount(ItemDef));
                    body.inventory.RemoveItem(ItemDef, playerCharacterMaster.master.GetBody().inventory.GetItemCount(ItemDef));
                    CharacterMasterNotificationQueue.SendTransformNotification(playerCharacterMaster.master, ItemDef.itemIndex, GOTCE.Items.NoTier.DelicatestWatch.Instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                }
            }
            orig(self, activator);
        }

        public void Damage(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body && body.inventory)
            {
                if (body.inventory.GetItemCount(ItemDef) > 0)
                {
                    args.damageMultAdd += 2f + 1f * (body.inventory.GetItemCount(ItemDef) - 1);
                }
            }
        }
    }
}