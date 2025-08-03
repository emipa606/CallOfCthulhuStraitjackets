using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cthulhu.Detour;

internal static class _MentalBreaker
{
    private static FieldInfo _pawn;

    private static Pawn GetPawn(this MentalBreaker _this)
    {
        if (_pawn != null)
        {
            return (Pawn)_pawn.GetValue(_this);
        }

        _pawn = typeof(MentalBreaker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
        if (_pawn == null)
        {
            Log.ErrorOnce("Unable to reflect MentalBreaker.pawn!", 215432421);
        }

        return (Pawn)_pawn?.GetValue(_this);
    }

    private static MentalBreakIntensity GetCurrentDesiredMoodBreakIntensity(this MentalBreaker _this)
    {
        var result = typeof(MentalBreaker).GetProperty("CurrentDesiredMoodBreakIntensity",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return (MentalBreakIntensity)result?.GetValue(_this, null)!;
    }

    private static IEnumerable<MentalBreakDef> GetCurrentPossibleMoodBreaks(this MentalBreaker _this)
    {
        var result = typeof(MentalBreaker).GetProperty("CurrentPossibleMoodBreaks",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return (IEnumerable<MentalBreakDef>)result?.GetValue(_this, null)!;
    }

    // Verse.MentalBreaker
    [Detour(typeof(MentalBreaker), bindingFlags = BindingFlags.Instance | BindingFlags.Public)]
    internal static bool TryDoRandomMoodCausedMentalBreak(this MentalBreaker _this)
    {
        if (!_this.CanDoRandomMentalBreaks || _this.GetPawn().Downed || !_this.GetPawn().Awake())
        {
            return false;
        }

        if (_this.GetPawn().Faction != Faction.OfPlayer &&
            _this.GetCurrentDesiredMoodBreakIntensity() != MentalBreakIntensity.Extreme)
        {
            return false;
        }

        if (!_this.GetCurrentPossibleMoodBreaks()
                .TryRandomElementByWeight(d => d.Worker.CommonalityFor(_this.GetPawn()), out var mentalBreakDef))
        {
            Log.Message(_this.GetCurrentDesiredMoodBreakIntensity().ToString());
            foreach (var def in _this.GetCurrentPossibleMoodBreaks())
            {
                Log.Message(def.ToString());
            }

            return false;
        }

        var method =
            typeof(MentalBreaker).GetMethod("RandomFinalStraw", BindingFlags.Instance | BindingFlags.NonPublic);

        Thought thought = null;
        if (method != null)
        {
            thought = (Thought)method.Invoke(_this, []);
        }

        var reason = thought?.LabelCap;


        if (_this.IsWearingStraitJacket())
        {
            if (Rand.Range(0, 100) < 95) //95% of the time
            {
                Utility.DebugReport("StraitJacket :: Mental Break Triggered");
                var stateDef = mentalBreakDef.mentalState;
                string label = "MentalBreakAvertedLetterLabel".Translate() + ": " + stateDef.beginLetterLabel;
                var text = string.Format(stateDef.beginLetter, _this.GetPawn().Label).AdjustedFor(_this.GetPawn())
                    .CapitalizeFirst();
                if (reason != null)
                {
                    text = $"{text}\n\n" + "MentalBreakReason".Translate(reason);
                    text = $"{text}\n\n" + "StraitjacketBenefit".Translate(_this.GetPawn().gender.GetPossessive(),
                        _this.GetPawn().gender.GetObjective(), $"{_this.GetPawn().gender.GetObjective()}self");
                }

                Find.LetterStack.ReceiveLetter(label, text, stateDef.beginLetterDef, _this.GetPawn());
                return false;
            }

            _this.StripStraitJacket();
            Messages.Message($"{_this.GetPawn().LabelCap} has escaped out of their straitjacket!", _this.GetPawn(),
                MessageTypeDefOf.NegativeEvent);
        }

        _this.GetPawn().mindState.mentalStateHandler
            .TryStartMentalState(mentalBreakDef.mentalState, reason, false, true);
        return true;
    }

    private static bool IsWearingStraitJacket(this MentalBreaker _this)
    {
        if (_this.GetPawn().apparel == null)
        {
            return false;
        }

        foreach (var clothing in _this.GetPawn().apparel.WornApparel)
        {
            if (clothing.def.defName == "Straitjacket")
            {
                return true;
            }
        }

        return false;
    }


    private static void StripStraitJacket(this MentalBreaker _this)
    {
        var pawn = _this.GetPawn();
        if (pawn.apparel == null)
        {
            return;
        }

        var clothingList = new List<Apparel>(pawn.apparel.WornApparel);
        foreach (var clothing in clothingList)
        {
            if (clothing.def.defName == "Straitjacket")
            {
                pawn.apparel.TryDrop(clothing, out _, pawn.Position);
            }
        }
    }
}