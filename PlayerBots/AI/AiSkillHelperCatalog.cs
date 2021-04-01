using PlayerBots.AI.SkillHelpers;
using RoR2;
using System;
using System.Collections.Generic;

namespace PlayerBots.AI
{
    class AiSkillHelperCatalog
    {
        private static AiSkillHelper DefaultSkillHelper = new DefaultSkillHelper();
        private static Dictionary<SurvivorIndex, AiSkillHelper> SkillHelperDict = new Dictionary<SurvivorIndex, AiSkillHelper>();

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
                    RegisterSkillHelper(Activator.CreateInstance(type) as AiSkillHelper);
                }
            }
        }

        public static void RegisterSkillHelper(AiSkillHelper skillHelper)
        {
            SurvivorIndex index = SurvivorIndex.None;
            SkillHelperSurvivor[] survivorAttributes = skillHelper.GetType().GetCustomAttributes(typeof(SkillHelperSurvivor), false) as SkillHelperSurvivor[];

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

            SkillHelperDict.Add(index, skillHelper);
        }

        public static AiSkillHelper GetSkillHelperByIndex(SurvivorIndex index)
        {
            AiSkillHelper helper;
            if (!SkillHelperDict.TryGetValue(index, out helper))
            {
                helper = DefaultSkillHelper;
            }
            return helper;
        }
    }

    class SkillHelperSurvivor : Attribute
    {
        public SkillHelperSurvivor(SurvivorIndex index)
        {
            Index = index;
        }

        public SkillHelperSurvivor(String bodyPrefabName)
        {
            Index = SurvivorIndex.None;
            BodyPrefabName = bodyPrefabName;
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
