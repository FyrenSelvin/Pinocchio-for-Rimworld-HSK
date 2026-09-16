using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Pinocchio
{
    [StaticConstructorOnStartup]
    internal static class PinocchioPatches
    {
        static PinocchioPatches()
        {
            new Harmony("fyren_selvin.pinocchio").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    internal static class PawnGenerator_GeneratePawn_PinocchioPatch
    {
        [HarmonyPostfix]
        private static void Postfix(PawnGenerationRequest request, Pawn __result)
        {
            if (__result != null && request.KindDef != null && request.KindDef.defName == "PinocchioColonist")
            {
                PinocchioUtility.Convert(__result, false);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    internal static class Pawn_SetFaction_PinocchioPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn __instance, Faction newFaction)
        {
            if (PinocchioMod.Settings.forceNewColonists && newFaction == Faction.OfPlayer && __instance.RaceProps.Humanlike)
            {
                PinocchioUtility.Convert(__instance, true);
            }
        }
    }

    internal static class PinocchioUtility
    {
        // A wooden prosthesis has 60% weight; the simple alternative has 40%.
        private static readonly string[] HandProstheses =
        {
            "WoodenHand", "WoodenHand", "WoodenHand",
            "SimpleProstheticHand", "SimpleProstheticHand",
            "HookHand"
        };

        private static readonly string[] LegProstheses =
        {
            "PegLeg", "PegLeg", "PegLeg",
            "SimpleProstheticLeg", "SimpleProstheticLeg"
        };

        internal static void Convert(Pawn pawn, bool setXenotype)
        {
            if (pawn == null || pawn.health == null || !pawn.RaceProps.Humanlike)
            {
                return;
            }

            if (setXenotype && pawn.genes != null)
            {
                pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.GetNamed("Pinocchio"));
            }

            PinocchioSettings settings = PinocchioMod.Settings;
            if (settings.replaceHands)
            {
                ReplaceAllMatchingParts(pawn, "Hand", HandProstheses, 1f);
            }

            if (settings.replaceLegs)
            {
                ReplaceAllMatchingParts(pawn, "Leg", LegProstheses, 1f);
            }

            if (settings.replaceInternalOrgans)
            {
                ReplaceAllMatchingParts(pawn, "Heart", new[] { "LifesupportHeart" }, settings.internalOrganReplacementChance);
                ReplaceAllMatchingParts(pawn, "Lung", new[] { "LifesupportLung" }, settings.internalOrganReplacementChance);
                ReplaceAllMatchingParts(pawn, "Kidney", new[] { "LifesupportKidney" }, settings.internalOrganReplacementChance);
                ReplaceAllMatchingParts(pawn, "Liver", new[] { "LifesupportLiver" }, settings.internalOrganReplacementChance);
                ReplaceAllMatchingParts(pawn, "Stomach", new[] { "LifesupportStomach" }, settings.internalOrganReplacementChance);
            }

            if (settings.replaceHeadParts)
            {
                ReplaceOneMatchingPart(pawn, "Eye", "EyePatch", 0.5f);
                ReplaceOneMatchingPart(pawn, "Jaw", "Denture", 0.5f);
            }
        }

        private static void ReplaceAllMatchingParts(Pawn pawn, string bodyPartDefName, IReadOnlyList<string> prostheses, float chance)
        {
            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts.Where(part => part.def.defName == bodyPartDefName))
            {
                if (Rand.Chance(chance))
                {
                    ReplacePartIfPossible(pawn, part, prostheses.RandomElement());
                }
            }
        }

        private static void ReplaceOneMatchingPart(Pawn pawn, string bodyPartDefName, string prosthesis, float chance)
        {
            if (!Rand.Chance(chance))
            {
                return;
            }

            List<BodyPartRecord> candidates = pawn.RaceProps.body.AllParts
                .Where(part => part.def.defName == bodyPartDefName && CanReplace(pawn, part))
                .ToList();
            if (candidates.Count > 0)
            {
                ReplacePartIfPossible(pawn, candidates.RandomElement(), prosthesis);
            }
        }

        private static void ReplacePartIfPossible(Pawn pawn, BodyPartRecord part, string prosthesisDefName)
        {
            if (!CanReplace(pawn, part))
            {
                return;
            }

            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(prosthesisDefName), part);
        }

        private static bool CanReplace(Pawn pawn, BodyPartRecord part)
        {
            return !pawn.health.hediffSet.PartIsMissing(part)
                && !pawn.health.hediffSet.hediffs.Any(hediff => hediff.Part == part && hediff.def.addedPartProps != null);
        }
    }
}
