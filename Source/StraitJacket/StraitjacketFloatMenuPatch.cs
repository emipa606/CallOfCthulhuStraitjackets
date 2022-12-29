using System;
using System.Collections.Generic;
using JecsTools;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace StraitJacket;

public class StraitjacketFloatMenuPatch : FloatMenuPatch
{
    public override IEnumerable<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>
        GetFloatMenus()
    {
        var floatMenus = new List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>();

        var straitjacketCondition = new _Condition(_ConditionType.IsType, typeof(Pawn));

        List<FloatMenuOption> StraitjacketFunc(Vector3 clickPos, Pawn pawn, Thing curThing)
        {
            var opts = new List<FloatMenuOption>();
            var target = curThing as Pawn;
            if (pawn == target || pawn.Dead || pawn.Downed)
            {
                return null;
            }

            var c = clickPos.ToIntVec3();
            if (!(target?.RaceProps?.Humanlike ?? false))
            {
                return opts;
            }

            //Let's proceed if our 'actor' is capable of manipulation
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return opts;
            }

            //Does the target have a straitjacket?
            //We can help them remove the straitjacket.
            if (target.apparel?.WornApparel?.FirstOrDefault(x => x.def == StraitjacketDefOf.ROM_Straitjacket) != null)
            {
                if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                {
                    opts.Add(new FloatMenuOption(
                        "CannotRemoveStraitjacket".Translate() + " (" + "NoPath".Translate() + ")", null));
                }
                else if (!pawn.CanReserve(target))
                {
                    opts.Add(new FloatMenuOption("CannotRemoveStraitjacket".Translate() + ": " + "Reserved".Translate(),
                        null));
                }
                else
                {
                    void Action()
                    {
                        var job = new Job(StraitjacketDefOf.ROM_TakeOffStraitjacket, target) { count = 1 };
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
                    return opts;
                }

                if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                {
                    opts.Add(new FloatMenuOption(
                        "CannotForceStraitjacket".Translate() + " (" + "NoPath".Translate() + ")", null));
                }
                else if (!pawn.CanReserve(target))
                {
                    opts.Add(new FloatMenuOption("CannotForceStraitjacket".Translate() + ": " + "Reserved".Translate(),
                        null));
                }
                else
                {
                    void Action()
                    {
                        var straitjacket = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                            ThingRequest.ForDef(StraitjacketDefOf.ROM_Straitjacket), PathEndMode.Touch,
                            TraverseParms.For(pawn));
                        var job = new Job(StraitjacketDefOf.ROM_ForceIntoStraitjacket, target, straitjacket)
                            { count = 1, locomotionUrgency = LocomotionUrgency.Sprint };
                        pawn.jobs.TryTakeOrderedJob(job);
                    }

                    opts.Add(new FloatMenuOption("ForceStraitjacketUpon".Translate(target.LabelCap), Action,
                        MenuOptionPriority.High, null, target));
                }
            }

            return opts;
        }

        var
            curSec = new KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>
                (straitjacketCondition, StraitjacketFunc);
        floatMenus.Add(curSec);
        return floatMenus;
    }
}