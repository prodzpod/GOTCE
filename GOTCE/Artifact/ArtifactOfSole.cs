using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API;

namespace GOTCE.Artifact {

    public class ArtifactOfSole : ArtifactBase<ArtifactOfSole>
    {
        public override string ArtifactName => "Artifact Of Sole";

        public override string ArtifactLangTokenName => "GOTCE_ArtifactOfSole";

        public override string ArtifactDescription => "Allies bleed out while moving.";

        public override Sprite ArtifactEnabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Artifact/artifactofblindnesson.png");

        public override Sprite ArtifactDisabledIcon => Main.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Artifact/artifactofblindnesson.png");

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.Start += (orig, self) => {
                orig(self);
                if (RunArtifactManager.instance.IsArtifactEnabled(ArtifactDef) && NetworkServer.active && self.playerCharacterMasterController) {
                    Weak com = self.gameObject.GetComponent<Weak>();
                    if (!com) {
                        self.gameObject.AddComponent<Weak>();
                    }
                }
            };
        }

    }

    public class Weak : MonoBehaviour {
        float damageMult = 0.2f;
        float delay = 0.5f;
        float stopwatch = 0f;
        CharacterMaster master;
        public void Start() {
            master = gameObject.GetComponent<CharacterMaster>();
        }
        public void FixedUpdate() {
            if (NetworkServer.active) {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= delay) {
                    stopwatch = 0f;
                    CharacterBody body = master.GetBody();
                    if (body) {
                        float damage = body.characterMotor.velocity.magnitude * damageMult;
                        if (damage >= 1) {
                            DamageInfo info = new();
                            info.attacker = null;
                            info.inflictor = null;
                            info.damageType = DamageType.NonLethal;
                            info.damage = damage;
                            info.damageColorIndex = DamageColorIndex.Bleed;

                            body.healthComponent.TakeDamage(info);

                            body.AddTimedBuff(RoR2Content.Buffs.Bleeding, 1f);
                        }

                    }
                }
            }
        }
    }
}