using System;
using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace StraitJacket;

/*
 * 
 *  Harmony Classes
 *  ===============
 *  Harmony is a system developed by pardeike (aka Brrainz).
 *  It allows us to use pre/post method patches instead of using detours.
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
            new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod("SetNextOptimizeTickPreFix")));
        harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Gear), "InterfaceDrop"),
            new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod("InterfaceDropPreFix")));
        harmony.Patch(AccessTools.Method(typeof(MentalBreaker), "get_CurrentPossibleMoodBreaks"), null,
            new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod("CurrentPossibleMoodBreaksPostFix")));
        //harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null, new HarmonyMethod(typeof(HarmonyStraitJacket).GetMethod("AddHumanlikeOrdersPostFix")));
    }

    // Verse.MentalBreaker
    public static void CurrentPossibleMoodBreaksPostFix(MentalBreaker __instance,
        ref IEnumerable<MentalBreakDef> __result)
    {
        //Declare variables for the process
        var pawn = (Pawn)AccessTools.Field(typeof(MentalBreaker), "pawn").GetValue(__instance);

        //IsWearingStraitJacket
        if (pawn?.apparel?.WornApparel?.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) == null)
        {
            return;
        }

        var thought = (Thought)AccessTools.Method(typeof(MentalBreaker), "RandomFinalStraw")
            .Invoke(__instance, new object[] { });
        var reason = thought?.LabelCap ?? "";

        //Reset the mind state because we probably tried to start something before this process started.
        //pawn.mindState.mentalStateHandler.Reset();

        if (!(__result?.TryRandomElementByWeight(d => d.Worker.CommonalityFor(pawn), out var mentalBreakDef) ?? false))
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
            var text = string.Format(stateDef.beginLetter, pawn.Label).AdjustedFor(pawn).CapitalizeFirst();
            text = text + "\n\n" + "StraitjacketBenefit".Translate(pawn.gender.GetPossessive(),
                pawn.gender.GetObjective(), pawn.gender.GetObjective() + "self");

            Find.LetterStack.ReceiveLetter(label, text, stateDef.beginLetterDef, pawn);
            __result = new List<MentalBreakDef>();
            return;
        }

        //StripStraitJacket
        if (pawn.apparel?.WornApparel?.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) is not
            { } clothing)
        {
            return;
        }

        if (pawn.apparel?.TryDrop(clothing, out _, pawn.Position) == null)
        {
            return;
        }

        Messages.Message("StraitjacketEscape".Translate(pawn.LabelCap),
            MessageTypeDefOf.ThreatBig); // MessageSound.SeriousAlert);
        pawn.mindState.mentalStateHandler.TryStartMentalState(mentalBreakDef.mentalState, reason, false,
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
            .Invoke(__instance, Array.Empty<object>());
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
            MessageTypeDefOf.RejectInput); //MessageSound.RejectInput);
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
        if (pawn == null)
        {
            return true;
        }

        if (pawn.outfits == null)
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

        if (wornApparel.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) != null)
        {
            return false;
        }

        return true;
    }

    // RimWorld.FloatMenuMakerMap
    /*
     *  PostFix
     * 
     *  This code adds to the float menu list.
     * 
     *  Adds:
     *    + Force straitjacket on _____
     *    + Help _____ out of straitjacket
     * 
     */
    public static void AddHumanlikeOrdersPostFix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
    {
        var c = IntVec3.FromVector3(clickPos);
        foreach (var current in c.GetThingList(pawn.Map))
        {
            if (current is not Pawn target || pawn == target || pawn.Dead || pawn.Downed)
            {
                continue;
            }

            //We sadly can't handle aggro mental states or non-humanoids.
            if (!(target.RaceProps?.Humanlike ?? false) || target.InAggroMentalState)
            {
                continue;
            }

            //Let's proceed if our 'actor' is capable of manipulation
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                continue;
            }

            //Does the target have a straitjacket?
            //We can help them remove the straitjacket.
            if (target.apparel?.WornApparel?.FirstOrDefault(x =>
                    x.def == StraitjacketDefOf.ROM_Straitjacket) != null)
            {
                if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                {
                    opts.Add(new FloatMenuOption(
                        "CannotRemoveStraitjacket".Translate() + " (" + "NoPath".Translate() + ")", null));
                }
                else if (!pawn.CanReserve(target))
                {
                    opts.Add(new FloatMenuOption(
                        "CannotRemoveStraitjacket".Translate() + ": " + "Reserved".Translate(), null));
                }
                else
                {
                    void Action()
                    {
                        var job = new Job(StraitjacketDefOf.ROM_TakeOffStraitjacket, target)
                        {
                            count = 1
                        };
                        pawn.jobs.TryTakeOrderedJob(job);
                    }

                    opts.Add(new FloatMenuOption("RemoveStraitjacket".Translate(target.LabelCap), Action,
                        MenuOptionPriority.High, null, target));
                }
            }
            //We can put one on!
            else
            {
                if (pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel)
                        .FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) == null)
                {
                    continue;
                }

                if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                {
                    opts.Add(new FloatMenuOption(
                        "CannotForceStraitjacket".Translate() + " (" + "NoPath".Translate() + ")",
                        null));
                }
                else if (!pawn.CanReserve(target))
                {
                    opts.Add(new FloatMenuOption(
                        "CannotForceStraitjacket".Translate() + ": " + "Reserved".Translate(), null));
                }
                else
                {
                    void Action()
                    {
                        var straitjacket = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                            ThingRequest.ForDef(StraitjacketDefOf.ROM_Straitjacket), PathEndMode.Touch,
                            TraverseParms.For(pawn));
                        var job = new Job(StraitjacketDefOf.ROM_ForceIntoStraitjacket, target, straitjacket)
                        {
                            count = 1,
                            locomotionUrgency = LocomotionUrgency.Sprint
                        };
                        pawn.jobs.TryTakeOrderedJob(job);
                    }

                    opts.Add(new FloatMenuOption("ForceStraitjacketUpon".Translate(target.LabelCap), Action,
                        MenuOptionPriority.High, null, target));
                }
            }
        }
    }

    // Verse.MentalBreaker
    /*
     *  PreFix
     * 
     *  By calling this code first, we can check if the pawn involved is wearing a straitjacket.
     *  If the colonist is wearing a straitjacket, do not trigger a standard mental break.
     *  Instead, declare te mental break averted.
     * 
     */
    public static bool TryDoRandomMoodCausedMentalBreakPreFix(MentalBreaker __instance)
    {
        //Declare variables for the process
        var pawn = (Pawn)AccessTools.Field(typeof(MentalBreaker), "pawn").GetValue(__instance);

        //IsWearingStraitJacket
        var isWearingStraitJacket = false;
        if (pawn.apparel != null)
        {
            foreach (var clothing in pawn.apparel.WornApparel)
            {
                if (clothing.def == StraitjacketDefOf.ROM_Straitjacket)
                {
                    isWearingStraitJacket = true;
                }
            }
        }

        if (!isWearingStraitJacket)
        {
            return true;
        }

        var thought = (Thought)AccessTools.Method(typeof(MentalBreaker), "RandomMentalBreakReason")
            .Invoke(__instance, new object[] { });
        var mentalBreaksList = (IEnumerable<MentalBreakDef>)AccessTools
            .Property(typeof(MentalBreaker), "CurrentPossibleMoodBreaks").GetValue(__instance, null);
        var reason = thought?.LabelCap;

        //Reset the mind state because we probably tried to start something before this process started.
        pawn.mindState.mentalStateHandler.Reset();


        if (!mentalBreaksList.TryRandomElementByWeight(d => d.Worker.CommonalityFor(pawn), out var mentalBreakDef))
        {
            return false;
        }

        if (Rand.Range(0, 100) < 95) //95% of the time
        {
            Utility.DebugReport("StraitJacket :: Mental Break Triggered");
            var stateDef = mentalBreakDef.mentalState;
            string label = "MentalBreakAvertedLetterLabel".Translate() + ": " + stateDef.beginLetterLabel;
            var text = string.Format(stateDef.beginLetter, pawn.Label).AdjustedFor(pawn).CapitalizeFirst();
            if (reason != null)
            {
                text = text + "\n\n" + "StraitjacketBenefit".Translate(pawn.gender.GetPossessive(),
                    pawn.gender.GetObjective(), pawn.gender.GetObjective() + "self");
            }

            Find.LetterStack.ReceiveLetter(label, text, stateDef.beginLetterDef, pawn);
            return false;
        }

        //StripStraitJacket
        if (pawn.apparel != null)
        {
            var clothingList = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (var clothing in clothingList)
            {
                if (clothing.def == StraitjacketDefOf.ROM_Straitjacket)
                {
                    pawn.apparel.TryDrop(clothing, out _, pawn.Position);
                }
            }
        }

        Messages.Message("StraitjacketEscape".Translate(pawn.LabelCap),
            MessageTypeDefOf.ThreatBig); // MessageSound.SeriousAlert);

        pawn.mindState.mentalStateHandler.TryStartMentalState(mentalBreakDef.mentalState, reason, false, true);
        return false;
    }
}