using System.Collections;
using System;
using UnityEngine;
using System.Globalization;
using System.Threading;

namespace FalloutChems
{


    public class FalloutChems : MonoBehaviour
    {
        // For stuff like below, this is why C++ exists. Unnecessary computation time
        // when it could be done at compile time, aka constexpr. This could return a 
        // constant value as the color doesn't change at runtime, but C# doesn't yet
        // have support for this as far as I can tell.
        // Small method for converting RGB to float using colour pickers.
        public static Color rgba(int r, int g, int b, int a)
        {
            return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, a == 0 ? 0 : (float)a / 255f);
        }
        public static Color rgb(int r, int g, int b)
        {
            return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
        }
        public static string ModTag = "[PRMods]"; //Whilst not required, modded items should contain 'tags' at the end of their names to prevent errors in which two mods have an item of the same name.
        public static string NameTag = " - Possibly Remember";
        public static void Main()
        {
            ModAPI.RegisterLiquid(Stimulant.Stimpack.ID, new Stimulant.Stimpack());
            ModAPI.Register(new Modification()
            {
                OriginalItem = ModAPI.FindSpawnable("Life Syringe"),
                NameOverride = "Stimpack" + ModTag,
                DescriptionOverride = "A stimpack from the Fallout series.",
                CategoryOverride = ModAPI.FindCategory("Biohazard"),
                ThumbnailOverride = ModAPI.LoadSprite("sprites/stimpack_thumbnail.png"),
                AfterSpawn = (Instance) => //yeah, a lot of unused code, it's hard to change a syringe model, first mod and all
                {
                    Destroy(Instance.GetComponent<LifeSyringe>());
                    Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("sprites/stimpack.png", 3f, true);
                    Instance.GetOrAddComponent<Stimulant>();
                    Instance.GetOrAddComponent<Stimulant>().use = ModAPI.LoadSound("./sfx/UseStoicSlurp.mp3");
                }
            });
            ModAPI.RegisterLiquid(BuffoutBase.Buffout.ID, new BuffoutBase.Buffout());
            ModAPI.Register(new Modification()
            {
                OriginalItem = ModAPI.FindSpawnable("Life Syringe"),
                NameOverride = "Buffout" + ModTag,
                DescriptionOverride = "A buffout from the Fallout series.",
                CategoryOverride = ModAPI.FindCategory("Biohazard"),
                ThumbnailOverride = ModAPI.LoadSprite("sprites/stimpack_thumbnail.png"),
                AfterSpawn = (Instance) => //yeah, a lot of unused code, it's hard to change a syringe model, first mod and all
                {
                    Destroy(Instance.GetComponent<SyringeBehaviour>());
                    Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("sprites/stimpack.png", 3f, true);
                    Instance.GetOrAddComponent<BuffoutBase>();
                }
            });
            ModAPI.RegisterLiquid(JetBase.Jet.ID, new JetBase.Jet());
            ModAPI.Register(new Modification()
            {
                OriginalItem = ModAPI.FindSpawnable("Life Syringe"),
                NameOverride = "Jet" + ModTag,
                DescriptionOverride = "A Jet from the Fallout series.",
                CategoryOverride = ModAPI.FindCategory("Biohazard"),
                ThumbnailOverride = ModAPI.LoadSprite("sprites/stimpack_thumbnail.png"),
                AfterSpawn = (Instance) => //yeah, a lot of unused code, it's hard to change a syringe model, first mod and all
                {
                    Destroy(Instance.GetComponent<SyringeBehaviour>());
                    Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("sprites/stimpack.png", 3f, true);
                    Instance.GetOrAddComponent<JetBase>();
                }
            });
        }
    }
    public class BuffoutBase : SyringeBehaviour
    {
        public override string GetLiquidID() => "Stimulant";

        public class Buffout : TemporaryBodyLiquid
        {
            const string BuffoutID = "Buffout ID [PRMods]";
            public const string ID = "Buffout";
            public override float RemovalChancePerSecond { get; } = 1f;
            public override bool ShouldCallOnEnterEveryUpdate => false;
            public Buffout()
            {
                Color = FalloutChems.rgba(234, 182, 118,0);
            }

            public override void OnEnterLimb(LimbBehaviour limb)
            {
                // considered using a lambda here, but I have to keep reminding myself
                // that this is NOT C++, plus the performance overhead.
                limb.BaseStrength = +0.01f;
                limb.ImpactPainMultiplier -= 0.05f;
            }

            public override void OnEnterContainer(BloodContainer container)
            {
            }

            public override void OnExitContainer(BloodContainer container)
            {
            }
        }
    }

    public class JetBase : SyringeBehaviour
    {
        public override string GetLiquidID() => "Jet";

        public class Jet : TemporaryBodyLiquid
        {
            const string JetID = "Jet ID [PRMods]";
            public const string ID = "Jet";

            public override float RemovalChancePerSecond { get; } = 0.02f;
            public override bool ShouldCallOnEnterEveryUpdate => true;

            public Jet()
            {
                Color = FalloutChems.rgba(79, 57, 133, 0);
            }
            public override void OnUpdate(BloodContainer c)
            {
                base.OnUpdate(c);
            }
            public override void OnEnterLimb(LimbBehaviour limb)
            {
                limb.ImpactPainMultiplier -= 0.001f;
                limb.RegenerationSpeed -= 0.0001f;
                limb.Person.AdrenalineLevel += 0.2f;
                limb.SkinMaterialHandler.AcidProgress += 0.0001f;
                limb.CirculationBehaviour.InternalBleedingIntensity += 0.0001f;
            }
            public override void OnEnterContainer(BloodContainer container)
            {
            }

            public override void OnExitContainer(BloodContainer container)
            {
            }
        }
    }

    public class Stimulant : SyringeBehaviour
    {
        public AudioClip use;
        private bool used;
        
        public override void Use(ActivationPropagation a)
        {
            if (!used)
            {
                switch (a.Channel)
                {
                    default:
                        {
                            // *play sound*
                            StartCoroutine(Cooldown());
                            break;
                        }
                }
            }
        }

        IEnumerator Cooldown()
        {
            used = true;
            GetComponent<AudioSource>().enabled = true;
            GetComponent<AudioSource>().PlayOneShot(use, 1);
            PressureMode = PressureDirection.Push;
            yield return new WaitForSeconds(1.2f);
            GetComponent<AudioSource>().enabled = false;
            PressureMode = PressureDirection.None;
            used = false;
        }
        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock materialProperty;
        protected override void Awake()
        {
            PressureMode = PressureDirection.None;
            materialProperty = new MaterialPropertyBlock();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.GetPropertyBlock(materialProperty);
        }
        public override string GetLiquidID() => "Stimulant";
        
        public class Stimpack : TemporaryBodyLiquid // if you wanted this effect to be temporary, you can derive from TemporaryBodyLiquid instead
        {
            // declare a globally unique ID for this liquid
            const string StimpackID = "Stimulant ID [PRMods]";
            // provide the liquid ID for this syringe

            public const string ID = "Stimulant";

            public override float RemovalChancePerSecond { get; } = 1f;
            public override bool ShouldCallOnEnterEveryUpdate => false;

            public Stimpack()
            {
                Color = new Color(1f, 1f, 1f, 0f);
            }
            public override void OnEnterLimb(LimbBehaviour limb)
            {
                limb.Person.Consciousness = 1f;
                if (limb.CirculationBehaviour.GetAmountOfBlood() < 1)
                {
                    limb.CirculationBehaviour.AddLiquid(Liquid.GetLiquid(Blood.ID), 0.005f);
                }
                if (limb.Health > 0.75f)
                {
                    limb.CirculationBehaviour.HealBleeding();
                }
            }

            //Called every second by every container for every liquid it contains.
            public override void OnUpdate(BloodContainer c)
            {
                if (c is CirculationBehaviour circ)
                {
                    var limb = circ.Limb;
                    limb.Health += 2f;
                    limb.Person.PainLevel -= 0.05f;
                    limb.Person.ShockLevel -= 0.01f;
                    limb.Person.AdrenalineLevel += 0.075f;
                }
            }
            public override void OnEnterContainer(BloodContainer container)
            {
            }
            public override void OnExitContainer(BloodContainer container)
            {
            }
        }
        private Color GetColor()
        {
            Color computedColor = GetComputedColor();
            computedColor.a = Mathf.Clamp01(ScaledLiquidAmount);
            return computedColor;
        }
        protected override void OnWillRenderObject()
        {
            materialProperty.SetColor(ShaderProperties.Get("_LiquidColour"), GetColor());
            switch (PressureMode)
            {
                case PressureDirection.Push:
                    materialProperty.SetFloat(ShaderProperties.Get("_Direction"), 1f);
                    break;
                case PressureDirection.Pull:
                    break;
                default:
                    materialProperty.SetFloat(ShaderProperties.Get("_Direction"), 0f);
                    break;
            }

            spriteRenderer.SetPropertyBlock(materialProperty);
        }
    }
}

// Originally uploaded by 'Possibly Remember'. Do not reupload without their explicit permission.
