using PlayerBots.AI;
using PlayerBots.AI.SkillHelpers;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerBots.AI
{
    class AiSkillHelperCatalog
    {
        private static AiSkillHelper DefaultSkillHelper = new DefaultSkillHelper();
        private static Dictionary<SurvivorIndex, AiSkillHelper> SkillHelperDict = new Dictionary<SurvivorIndex, AiSkillHelper>();

        static AiSkillHelperCatalog()
        {
            Populate();
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
                index = survivorAttributes[0].Index;
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

        public SurvivorIndex Index
        {
            get;
            set;
        }
    }
}
