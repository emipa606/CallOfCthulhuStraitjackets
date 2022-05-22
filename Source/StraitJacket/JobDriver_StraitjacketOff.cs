using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace StraitJacket;

public class JobDriver_StraitjacketOff : JobDriver
{
    private const TargetIndex TakeeIndex = TargetIndex.A;

    protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool yeaaaa)
    {
        return pawn.Reserve(job.targetA, job);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);
        var toil2 = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 1000
        };
        toil2.WithProgressBarToilDelay(TargetIndex.A);
        toil2.initAction = delegate { PawnUtility.ForceWait((Pawn)TargetA.Thing, toil2.defaultDuration, pawn); };
        yield return toil2;

        yield return new Toil
        {
            initAction = delegate
            {
                var straitjacket =
                    Takee.apparel.WornApparel.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket);
                if (straitjacket != null)
                {
                    Takee.apparel.TryDrop(straitjacket, out _, Takee.Position);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}