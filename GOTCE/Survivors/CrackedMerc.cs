using System;
using RoR2;
using UnityEngine;

namespace GOTCE.Survivors {
    public class CrackedMerc : SurvivorBase<CrackedMerc> {
        public override string bodypath => Utils.Paths.GameObject.MercBody;
        public override string name => "CrackedMercBody";
        public override bool clone => true;
        public static GameObject DisplayPrefab;
        public static Texture2D texCrecDiffuse;
        public static Material matCrecDiffuse;

        public override void Modify()
        {
            base.Modify();

            Material mercMat = Utils.Paths.Material.matMerc.Load<Material>();

            texCrecDiffuse = Main.SecondaryAssets.LoadAsset<Texture2D>("crecDiffuse.png");
            matCrecDiffuse = DuplicateMat(mercMat, "matCrec", texCrecDiffuse);

            GameObject dml = Utils.Paths.GameObject.DisplayMissileRack.Load<GameObject>();

            CharacterBody body = prefab.GetComponent<CharacterBody>();
            body.baseNameToken = "GOTCE_CRACKEDMERC_NAME".Add("Cracked Mercenary");
            body.bodyColor = Color.yellow;
            body.baseDamage = 14f;

            SkillLocator locator = prefab.GetComponent<SkillLocator>();
            locator.passiveSkill.skillNameToken = "GOTCE_CRACKEDMERC_PASSIVE_NAME".Add("The OP Build");
            locator.passiveSkill.skillDescriptionToken = "GOTCE_CRACKEDMERC_PASSIVE_DESC".Add("Your <style=cIsDamage>base damage</style> is increased by <style=cIsUtility>10%</style> for every <style=cIsDamage>Berserker's Pauldron</style> you have.");
            locator.passiveSkill.icon = null;

            LanguageAPI.Add("GOTCE_CRACKEDMERC_SUBTITLE", "The Volatile");
            LanguageAPI.Add("GOTCE_CRACKEDMERC_WIN", "And so he left, fuse still ticking.");
            LanguageAPI.Add("GOTCE_CRACKEDMERC_FAIL", "TBD");
            LanguageAPI.Add("GOTCE_CRACKEDMERC_DESC", "TBD");

            LanguageAPI.Add("GOTCE_DisposableVisions_NAME", "Visions of Disposability");
            LanguageAPI.Add("GOTCE_DisposableVisions_DESC", "Fire a <style=cIsUtility>missile</style> for <style=cIsDamage>300% damage</style>. <style=cIsUtility>Hold up to 12</style>.");

            LanguageAPI.Add("GOTCE_DisposableRising_NAME", "Rising Disposable Thunder Surge");
            LanguageAPI.Add("GOTCE_DisposableRising_DESC", "Propel yourself into the air, <style=cIsDamage>slashing for 300% damage</style> and <style=cIsDamage>firing a missile for 300% damage</style>.");

            LanguageAPI.Add("GOTCE_DisposableEgg_NAME", "Volcanic Disposable Missile Launcher Assault");
            LanguageAPI.Add("GOTCE_DisposableEgg_DESC", "Transform into a blazing fast missile, <style=cIsDamage>dealing 100,000% damage</style> on detonation.");

            LanguageAPI.Add("GOTCE_DisposableEvis_NAME", "Slicing Eviscerating Unstable Strides of Windy Disposability");
            LanguageAPI.Add("GOTCE_DisposableEvis_DESC", "Transform into a <style=cIsUtility>storm of blades</style>, repeating <style=cIsDamage>striking for 300%</style> damage and <style=cIsUtility>firing missiles</style>.");

            ModelLocator model = prefab.GetComponent<ModelLocator>();
            SwapMaterials(prefab, mercMat, matCrecDiffuse);
            SwapMaterials(prefab, matCrecDiffuse, Utils.Paths.Material.matGupBody.Load<Material>(), true);

            ChildLocator loc = model.modelTransform.GetComponent<ChildLocator>();

            Transform head = loc.FindChild("Head");
            head.gameObject.AddComponent<ScaleNullifer>();

            GameObject headDml = GameObject.Instantiate<GameObject>(dml, head);
            headDml.transform.localPosition = new Vector3(0, -20f, 0);
            headDml.transform.localRotation = Quaternion.Euler(-20, 180, 0);
            headDml.transform.localScale = 100f * Vector3.one;

            Transform chest = loc.FindChild("Chest");

            /*GameObject chestDml = GameObject.Instantiate<GameObject>(dml, chest);
            chestDml.transform.localPosition = Vector3.zero;


            Transform sword = chest.Find("SwingCenter").Find("SwordBase");

            GameObject swordDml = GameObject.Instantiate<GameObject>(dml, sword);
            swordDml.transform.localPosition = Vector3.zero;

            sword.Find("SwordLength.1").gameObject.AddComponent<ScaleNullifer>();*/

            GameObject.Destroy(model.modelTransform.GetComponent<ModelSkinController>());

            ReplaceSkill(locator.primary, Skills.DisposableVisions.Instance.SkillDef);
            ReplaceSkill(locator.secondary, Skills.DisposableRising.Instance.SkillDef);
            ReplaceSkill(locator.utility, Skills.DisposableEgg.Instance.SkillDef);
            ReplaceSkill(locator.special, Skills.DisposableEvis.Instance.SkillDef);

            RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (self.baseNameToken != "GOTCE_CRACKEDMERC_NAME") return;

                args.damageMultAdd += 0.1f * self.inventory.GetItemCount(RoR2Content.Items.WarCryOnMultiKill);
            };

            DisplayPrefab = PrefabAPI.InstantiateClone(model.modelTransform.gameObject, "CrackedMercDisplay", false);
        }

        public class ScaleNullifer : MonoBehaviour
        {
            public void Update()
            {
                base.transform.localScale = 0.01f * Vector3.one;
            }

            public void LateUpdate()
            {
                base.transform.localScale = 0.01f * Vector3.one;
            }
        }

        public override void PostCreation()
        {
            base.PostCreation();
            SurvivorDef surv = new SurvivorDef
            {
                bodyPrefab = prefab,
                descriptionToken = "GOTCE_CRACKEDMERC_DESC",
                displayPrefab = DisplayPrefab,
                primaryColor = Color.yellow,
                cachedName = "GOTCE_CRACKEDMERC_NAME",
                unlockableDef = null,
                desiredSortPosition = 16,
                mainEndingEscapeFailureFlavorToken = "GOTCE_CRACKEDMERC_FAIL",
                outroFlavorToken = "GOTCE_CRACKEDMERC_WIN",
            };

            ContentAddition.AddBody(prefab);
            ContentAddition.AddSurvivorDef(surv);
        }
    }
}