using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace GOTCE.Items.White
{
    public class EmpathyC4 : ItemBase<EmpathyC4>
    {
        public override string ConfigName => "Empathy C4";

        public override string ItemName => "Empathy C4";

        public override string ItemLangTokenName => "GOTCE_EmpathyC4";

        public override string ItemPickupDesc => "Gain";

        public override string ItemFullDescription => "Gain";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override Enum[] ItemTags => new Enum[] { ItemTag.Damage, ItemTag.Healing, GOTCETags.Crit, GOTCETags.FovRelated, ItemTag.OnStageBeginEffect };

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
            RecalculateStatsAPI.GetStatCoefficients += Synergy;
        }

        public void Synergy(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args) {
            if (body.inventory) {
                float increase = 0.02f * GetCount(body);

                args.armorAdd += body.armor * (increase - 1);
                args.attackSpeedMultAdd += increase;
                args.moveSpeedMultAdd += increase;
                args.damageMultAdd += increase;
                args.moveSpeedMultAdd += increase;
                args.cooldownMultAdd -= increase;
                args.baseJumpPowerAdd += increase;
                args.baseHealthAdd += increase;
                args.shieldMultAdd += increase;
                args.baseRegenAdd += body.regen * (increase);
                args.critDamageMultAdd += increase;
                args.levelMultAdd += increase;
            }
        }
    }
}