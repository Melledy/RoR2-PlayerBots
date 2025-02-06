using PlayerBots.AI.SkillHelpers;
using RoR2;
using System;
using System.Collections.Generic;

namespace PlayerBots.AI
{
    class AiSkillHelperCatalog
    {
        private static Type DefaultSkillHelper = typeof(DefaultSkillHelper);
        private static Dictionary<SurvivorIndex, Type> SkillHelperDict = new Dictionary<SurvivorIndex, Type>();

        static AiSkillHelperCatalog()
        {

        }

        public static void Populate()
        {
            foreach (Type type in typeof(AiSkillHelperCatalog).Assembly.GetTypes())
            {
                var attribs = type.GetCustomAttributes(typeof(SkillHelperSurvivor), false);
                if (attribs != null && attribs.Length > 0)
                {
                    RegisterSkillHelper(type);
                }
            }
        }

        public static void RegisterSkillHelper(Type skillHelperType)
        {
            SurvivorIndex index = SurvivorIndex.None;
            SkillHelperSurvivor[] survivorAttributes = skillHelperType.GetCustomAttributes(typeof(SkillHelperSurvivor), false) as SkillHelperSurvivor[];

            if (survivorAttributes.Length > 0)
            {
                SkillHelperSurvivor skillHelperSurvivor = survivorAttributes[0];
                if (skillHelperSurvivor.Index != SurvivorIndex.None)
                {
                    index = skillHelperSurvivor.Index;
                }
                else if (skillHelperSurvivor.BodyPrefabName != null)
                {
                    if (PlayerBotUtils.TryGetSurvivorIndexByBodyPrefabName(skillHelperSurvivor.BodyPrefabName, out index))
                    {
                        string name = BodyCatalog.FindBodyPrefab(skillHelperSurvivor.BodyPrefabName).GetComponent<CharacterBody>().GetDisplayName().ToLower();
                        if (!PlayerBotManager.SurvivorDict.ContainsKey(name))
                        {
                            PlayerBotManager.SurvivorDict.Add(name, index);
                        }
                        if (skillHelperSurvivor.AllowRandom)
                        {
                            PlayerBotManager.RandomSurvivorsList.Add(index);
                        } 
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            SkillHelperDict.Add(index, skillHelperType);
        }

        public static AiSkillHelper CreateSkillHelper(SurvivorIndex index)
        {
            Type helperType;
            if (!SkillHelperDict.TryGetValue(index, out helperType))
            {
                helperType = DefaultSkillHelper;
            }
            return Activator.CreateInstance(helperType) as AiSkillHelper;
        }
    }

    class SkillHelperSurvivor : Attribute
    {
        public SkillHelperSurvivor(SurvivorIndex index)
        {
            Index = index;
            AllowRandom = true;
        }

        public SkillHelperSurvivor(String bodyPrefabName)
        {
            Index = SurvivorIndex.None;
            BodyPrefabName = bodyPrefabName;
            AllowRandom = true;
        }

        public SkillHelperSurvivor(String bodyPrefabName, bool allowRandom)
        {
            Index = SurvivorIndex.None;
            BodyPrefabName = bodyPrefabName;
            AllowRandom = allowRandom;
        }

        public SurvivorIndex Index
        {
            get;
            private set;
        }

        public string BodyPrefabName
        {
            get;
            private set;
        }

        public bool AllowRandom
        {
            get;
            private set;
        }
    }

    class CustomSurvivor : Attribute
    {
        public CustomSurvivor(String homepage, String perferredVersion)
        {
            this.Homepage = homepage;
            this.PerferredVersion = perferredVersion;
        }

        public String Homepage
        {
            get;
            private set;
        }

        public String PerferredVersion
        {
            get;
            private set;
        }
    }
}
