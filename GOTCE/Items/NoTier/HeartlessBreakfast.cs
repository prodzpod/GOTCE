using R2API;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.Networking;

namespace GOTCE.Items.NoTier
{
    public class HeartlessBreakfast : ItemBase<HeartlessBreakfast>
    {
        public override string ConfigName => "Heartless Breakfast";

        public override string ItemName => "Heartless Breakfast";

        public override string ItemLangTokenName => "GOTCE_HeartlessBreakfast";

        public override string ItemPickupDesc => "Does nothing.";

        public override string ItemFullDescription => "Does nothing.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist };

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
        }
    }
}