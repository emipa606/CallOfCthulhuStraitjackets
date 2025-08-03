using System.Collections.Generic;
using System.Diagnostics;
using JecsTools;
using RimWorld;
using Verse;
using Verse.AI;

namespace StraitJacket;

public class JobDriver_StraitjacketOn : JobDriver
{
    private const TargetIndex TakeeIndex = TargetIndex.A;

    private const TargetIndex StraitjacketIndex = TargetIndex.B;


    private Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    private Apparel Straitjacket => (Apparel)job.GetTarget(TargetIndex.B).Thing;

    // Verse.Pawn
    private static bool CheckAcceptStraitJacket(Pawn victim, Pawn arrester)
    {
        if (victim.Faction == arrester.Faction && !victim.InMentalState)
        {
            return true;
        }

        return arrester.TryGrapple(victim);
    }


    public override bool TryMakePreToilReservations(bool yeaa)
    {
        return pawn.Reserve(job.targetA, job) && pawn.Reserve(job.targetB, job);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate
            {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);

                var pawnToForceIntoStraitjacket = (Pawn)TargetA.Thing;
                if (pawnToForceIntoStraitjacket == null)
                {
                    return;
                }

                if (pawnToForceIntoStraitjacket.InAggroMentalState)
                {
                    return;
                }

                GenClamor.DoClamor(pawn, 10f, ClamorDefOf.Harm);
                if (!CheckAcceptStraitJacket(pawnToForceIntoStraitjacket, pawn))
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        var toil2 = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 500
        };
        toil2.WithProgressBarToilDelay(TargetIndex.A);
        toil2.initAction = delegate
        {
            var pawnToForceIntoStraitjacket = (Pawn)TargetA.Thing;

            if (pawnToForceIntoStraitjacket == null)
            {
                return;
            }

            if (!pawnToForceIntoStraitjacket.InAggroMentalState)
            {
                PawnUtility.ForceWait(pawnToForceIntoStraitjacket, toil2.defaultDuration, pawn);
            }
        };
        yield return toil2;

        yield return new Toil
        {
            initAction = delegate
            {
                Takee.apparel.Wear(Straitjacket);
                Takee.outfits.forcedHandler.SetForced(Straitjacket, true);
                var pawnJacketHediff =
                    Takee.health.hediffSet.GetFirstHediffOfDef(StraitjacketDefOf.ROM_RestainedByStraitjacket);
                if (pawnJacketHediff != null)
                {
                    return;
                }

                pawnJacketHediff = HediffMaker.MakeHediff(StraitjacketDefOf.ROM_RestainedByStraitjacket, Takee);
                Takee.health.AddHediff(pawnJacketHediff);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}