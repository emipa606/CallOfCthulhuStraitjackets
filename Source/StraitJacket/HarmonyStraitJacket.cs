using System;
using System.Collections.Generic;
using Cthulhu;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace StraitJacket;

/*
 *
 *  Harmony Classes
 *  ===============
 *  Harmony is a system developed by pardeike (aka Brrainz).
 *  It allows us to use pre- / post-method patches instead of using detours.
 *
 */
[StaticConstructorOnStartup]
internal static class HarmonyStraitJacket
{
    //Static Constructor
    /*
     * Contains 4 Harmony patches for 4 vanilla methods.
     * ===================
     *
     * [PREFIX] JobGiver_OptimizeApparel -> SetNextOptimizeTick
     * [POSTFIX] ITab_Pawn_Gear -> InterfaceDrop
     * [POSTFIX] MentalBreaker -> get_CurrentPossibleMoodBreaks
     * [POSTFIX] FloatMenuMakerMap -> AddHumanlikeOrders
     *
     */
    static HarmonyStraitJacket()
    {
        var harmony = new Harmony("rimworld.jecrell.straitjacket");
        harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), "SetNextOptimizeTick"),
            new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod(nameof(SetNextOptimizeTickPreFix))));
        harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Gear), "InterfaceDrop"),
            new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod(nameof(InterfaceDropPreFix))));
        harmony.Patch(AccessTools.Method(typeof(MentalBreaker), "get_CurrentPossibleMoodBreaks"), null,
            new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod(nameof(CurrentPossibleMoodBreaksPostFix))));
    }

    // Verse.MentalBreaker
    public static void CurrentPossibleMoodBreaksPostFix(MentalBreaker __instance,
        ref IEnumerable<MentalBreakDef> __result, Pawn ___pawn)
    {
        //IsWearingStraitJacket
        if (___pawn?.apparel?.WornApparel?.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) == null)
        {
            return;
        }

        var thought = (Thought)AccessTools.Method(typeof(MentalBreaker), "RandomFinalStraw")
            .Invoke(__instance, []);
        var reason = thought?.LabelCap ?? "";

        //Reset the mind state because we probably tried to start something before this process started.

        if (!(__result?.TryRandomElementByWeight(d => d.Worker.CommonalityFor(___pawn), out var mentalBreakDef) ??
              false))
        {
            return;
        }

        if (Rand.Range(0, 100) < 95) //95% of the time
        {
            Utility.DebugReport("StraitJacket :: Mental Break Triggered");
            var stateDef = mentalBreakDef?.mentalState ?? (Rand.Value > 0.5f
                ? DefDatabase<MentalStateDef>.GetNamed("Berserk")
                : DefDatabase<MentalStateDef>.GetNamed("Wander_Psychotic"));
            string label = "MentalBreakAvertedLetterLabel".Translate() + ": " + stateDef.beginLetterLabel;
            string text;
            try
            {
                text = string.Format(stateDef.beginLetter, ___pawn.Label);
            }
            catch (FormatException)
            {
                // Fallback: replace {0} if format placeholders don't match expected args
                text = stateDef.beginLetter.Replace("{0}", ___pawn.Label);
            }

            text = text.AdjustedFor(___pawn).CapitalizeFirst();
            text = text.Replace("{Pawn_nameDef}", ___pawn.Label);
            text = text.Replace("{Pawn_pronoun}", ___pawn.NameShortColored);
            text = text + "\n\n" + "StraitjacketBenefit".Translate(___pawn.gender.GetPossessive(),
                ___pawn.gender.GetObjective(), ___pawn.gender.GetObjective() + "self");

            Find.LetterStack.ReceiveLetter(label, text, stateDef.beginLetterDef, ___pawn);
            __result = new List<MentalBreakDef>();
            return;
        }

        //StripStraitJacket
        if (___pawn.apparel?.WornApparel?.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) is not
            { } clothing)
        {
            return;
        }

        if (___pawn.apparel?.TryDrop(clothing, out _, ___pawn.Position) == null)
        {
            return;
        }

        Messages.Message("StraitjacketEscape".Translate(___pawn.LabelCap),
            MessageTypeDefOf.ThreatBig); // MessageSound.SeriousAlert);
        ___pawn.mindState.mentalStateHandler.TryStartMentalState(mentalBreakDef.mentalState, reason, false,
            true);
        __result = new List<MentalBreakDef>();
    }

    // RimWorld.ITab_Pawn_Gear
    /*
     *  PreFix
     *
     *  Disables the drop button's effect if the user is wearing a straitjacket.
     *  A straitjacket user should not be able to take it off by themselves, right?
     *
     */
    public static bool InterfaceDropPreFix(ITab_Pawn_Gear __instance, Thing t)
    {
        var apparel = t as Apparel;
        var __pawn = (Pawn)AccessTools.Method(typeof(ITab_Pawn_Gear), "get_SelPawnForGear")
            .Invoke(__instance, []);
        if (__pawn == null)
        {
            return true;
        }

        if (apparel == null || __pawn.apparel == null || !__pawn.apparel.WornApparel.Contains(apparel))
        {
            return true;
        }

        if (apparel.def != StraitjacketDefOf.ROM_Straitjacket)
        {
            return true;
        }

        Messages.Message("CannotRemoveByOneself".Translate(__pawn.Label),
            MessageTypeDefOf.RejectInput);
        return false;
    }


    // RimWorld.JobGiver_OptimizeApparel
    /*
     *  PreFix
     *
     *  This code prevents prisoners/colonists from automatically changing
     *  out of straitjackets into other clothes.
     *
     */
    public static bool SetNextOptimizeTickPreFix(JobGiver_OptimizeApparel __instance, Pawn pawn)
    {
        if (pawn?.outfits == null)
        {
            return true;
        }

        var wornApparel = pawn.apparel.WornApparel;
        if (wornApparel == null)
        {
            return true;
        }

        if (wornApparel.Count <= 0)
        {
            return true;
        }

        return wornApparel.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) == null;
    }
}