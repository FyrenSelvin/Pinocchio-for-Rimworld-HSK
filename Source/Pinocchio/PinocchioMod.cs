using UnityEngine;
using Verse;

namespace Pinocchio
{
    public class PinocchioMod : Mod
    {
        internal static PinocchioSettings Settings;

        public PinocchioMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<PinocchioSettings>();
        }

        public override string SettingsCategory()
        {
            return "Pinocchio";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.Draw(inRect);
        }
    }

    public class PinocchioSettings : ModSettings
    {
        public bool replaceHands = true;
        public bool replaceLegs = true;
        public bool replaceInternalOrgans = true;
        public float internalOrganReplacementChance = 1f;
        public bool replaceHeadParts = true;
        public bool forceNewColonists;

        public void Draw(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("Заменять руки", ref replaceHands, "Случайно заменяет каждую кисть деревянной кистью, простым протезом или крюком.");
            listing.CheckboxLabeled("Заменять ноги", ref replaceLegs, "Случайно заменяет каждую ногу деревянной или простой ногой.");
            listing.CheckboxLabeled("Заменять внутренние органы", ref replaceInternalOrgans, "Заменяет сердце, лёгкие, почки, печень и желудок системами жизнеобеспечения.");
            listing.Label("Вероятность замены каждого внутреннего органа: " + Mathf.RoundToInt(internalOrganReplacementChance * 100f) + "%");
            internalOrganReplacementChance = listing.Slider(internalOrganReplacementChance, 0f, 1f);
            listing.CheckboxLabeled("Заменять глаза и челюсть", ref replaceHeadParts, "С вероятностью 50% ставит повязку на один глаз и зубной протез на челюсть.");
            listing.CheckboxLabeled("Принудительно превращать новых колонистов в Пиноккио", ref forceNewColonists, "Каждый новый человек, присоединившийся к фракции игрока, получит ксенотип Пиноккио и включённые протезы.");
            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref replaceHands, "replaceHands", true);
            Scribe_Values.Look(ref replaceLegs, "replaceLegs", true);
            Scribe_Values.Look(ref replaceInternalOrgans, "replaceInternalOrgans", true);
            Scribe_Values.Look(ref internalOrganReplacementChance, "internalOrganReplacementChance", 1f);
            Scribe_Values.Look(ref replaceHeadParts, "replaceHeadParts", true);
            Scribe_Values.Look(ref forceNewColonists, "forceNewColonists", false);
        }
    }
}
