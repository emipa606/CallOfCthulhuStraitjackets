using RimWorld;
using Verse;
using Verse.AI;

namespace StraitJacket;

public class Straitjacket_FloatMenuOptionProvider : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    protected override bool RequiresManipulation => true;

    protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
    {
        if (clickedPawn.Dead || clickedPawn.Downed)
        {
            return null;
        }

        if (!(clickedPawn.RaceProps?.Humanlike ?? false))
        {
            return null;
        }

        var hasJacket =
            clickedPawn.apparel?.WornApparel?.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) != null;

        if (hasJacket)
        {
            if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly))
            {
                return new FloatMenuOption("CannotRemoveStraitjacket".Translate() + " (" + "NoPath".Translate() + ")",
                    null);
            }

            return !context.FirstSelectedPawn.CanReserve(clickedPawn)
                ? new FloatMenuOption("CannotRemoveStraitjacket".Translate() + ": " + "Reserved".Translate(), null)
                : new FloatMenuOption("RemoveStraitjacket".Translate(clickedPawn.LabelCap), unEquipAction,
                    MenuOptionPriority.High, null, clickedPawn);

            void unEquipAction()
            {
                context.FirstSelectedPawn.jobs.TryTakeOrderedJob(
                    new Job(StraitjacketDefOf.ROM_TakeOffStraitjacket, clickedPawn) { count = 1 });
            }
        }

        if (clickedPawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel)
                .FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) == null)
        {
            return null;
        }

        if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly))
        {
            return new FloatMenuOption("CannotForceStraitjacket".Translate() + " (" + "NoPath".Translate() + ")", null);
        }

        if (!context.FirstSelectedPawn.CanReserve(clickedPawn))
        {
            return new FloatMenuOption("CannotForceStraitjacket".Translate() + ": " + "Reserved".Translate(), null);
        }

        return new FloatMenuOption("ForceStraitjacketUpon".Translate(clickedPawn.LabelCap), equipAction,
            MenuOptionPriority.High, null, clickedPawn);

        void equipAction()
        {
            var straitjacket = GenClosest.ClosestThingReachable(context.FirstSelectedPawn.Position,
                context.FirstSelectedPawn.Map,
                ThingRequest.ForDef(StraitjacketDefOf.ROM_Straitjacket), PathEndMode.Touch,
                TraverseParms.For(context.FirstSelectedPawn));
            var job = new Job(StraitjacketDefOf.ROM_ForceIntoStraitjacket, clickedPawn, straitjacket)
                { count = 1, locomotionUrgency = LocomotionUrgency.Sprint };
            context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
        }
    }
}