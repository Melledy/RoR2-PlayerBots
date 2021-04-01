using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerBots
{
    class IndexManager
    {
        public static EquipmentIndex CommandMissile;
        public static EquipmentIndex Lightning;
        public static EquipmentIndex DeathProjectile;
        public static EquipmentIndex BFG;
        public static EquipmentIndex CritOnUse;
        public static EquipmentIndex TeamWarCry;
        public static EquipmentIndex Blackhole;
        public static EquipmentIndex LifestealOnHit;
        public static EquipmentIndex PassiveHealing;
        public static EquipmentIndex Fruit;
        public static EquipmentIndex GainArmor;
        public static EquipmentIndex Cleanse;

        public static BuffIndex BuffFullCrit;
        public static BuffIndex BuffHealingDisabled;
        public static BuffIndex BuffOnFire;
        public static BuffIndex BuffTeamWarCry;

        public static void Build()
        {
            CommandMissile = EquipmentCatalog.FindEquipmentIndex("CommandMissile");
            Lightning = EquipmentCatalog.FindEquipmentIndex("Lightning");
            DeathProjectile = EquipmentCatalog.FindEquipmentIndex("DeathProjectile");
            BFG = EquipmentCatalog.FindEquipmentIndex("BFG");
            CritOnUse = EquipmentCatalog.FindEquipmentIndex("CritOnUse");
            TeamWarCry = EquipmentCatalog.FindEquipmentIndex("TeamWarCry");
            Blackhole = EquipmentCatalog.FindEquipmentIndex("Blackhole");
            LifestealOnHit = EquipmentCatalog.FindEquipmentIndex("LifestealOnHit");
            PassiveHealing = EquipmentCatalog.FindEquipmentIndex("PassiveHealing");
            Fruit = EquipmentCatalog.FindEquipmentIndex("Fruit");
            GainArmor = EquipmentCatalog.FindEquipmentIndex("GainArmor");
            Cleanse = EquipmentCatalog.FindEquipmentIndex("Cleanse");

            BuffFullCrit = BuffCatalog.FindBuffIndex("BuffFullCrit");
            BuffHealingDisabled = BuffCatalog.FindBuffIndex("BuffHealingDisabled");
            BuffOnFire = BuffCatalog.FindBuffIndex("BuffOnFire");
            BuffTeamWarCry = BuffCatalog.FindBuffIndex("BuffTeamWarCry");
        }
    }
}
