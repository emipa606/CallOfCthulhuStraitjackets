// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')
// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace StraitJacket;

internal class MapComponent_StraitJacket : MapComponent
{
    private int lazyTick = 750;
    private Map mapRecord;

    public MapComponent_StraitJacket(Map map) : base(map)
    {
        this.map = map;
        mapRecord = map;
    }

    public static MapComponent_StraitJacket GetComponent(Map map)
    {
        var result = map.components.OfType<MapComponent_StraitJacket>().FirstOrDefault();
        if (result != null)
        {
            return result;
        }

        result = new MapComponent_StraitJacket(map);
        map.components.Add(result);

        return result;
    }

    public override void MapComponentTick()
    {
        lazyTick--;
        if (lazyTick < 0)
        {
            lazyTick = 750;
            PerformStraitJacketCheck();
        }

        base.MapComponentTick();
    }

    // Verse.MapPawns
    public IEnumerable<Pawn> Prisoners(Map map)
    {
        return from x in map.mapPawns.AllPawns
            where x.IsPrisoner
            select x;
    }

    private void PerformStraitJacketCheck()
    {
        if (map.mapPawns == null)
        {
            return;
        }

        if (map.mapPawns.FreeColonists == null)
        {
            return;
        }

        var colonists = new HashSet<Pawn>(map.mapPawns.FreeColonists);
        var prisoners = new HashSet<Pawn>(Prisoners(map));
        var others = new HashSet<Pawn>(map.mapPawns.AllPawns.Where(x =>
            (x?.RaceProps?.Humanlike ?? false) && x.Faction != Faction.OfPlayer));
        var giveThoughtToAll = false;
        Pawn straightjackedPawn = null;
        Hediff pawnJacketHediff = null;

        //Check our prisoners first
        foreach (var p in prisoners.Concat(others))
        {
            if (p.apparel == null)
            {
                continue;
            }

            var jacketOn = false;
            foreach (var apparel in p.apparel.WornApparel)
            {
                if (apparel.def != StraitjacketDefOf.ROM_Straitjacket)
                {
                    continue;
                }

                jacketOn = true;
                //Log.Message("Straitjacket Prisoner Check");

                straightjackedPawn = p;
                p.needs.mood.thoughts.memories.TryGainMemory(StraitjacketDefOf.ROM_WoreStraitjacket);

                pawnJacketHediff =
                    p.health.hediffSet.GetFirstHediffOfDef(
                        StraitjacketDefOf.ROM_RestainedByStraitjacket);
                if (pawnJacketHediff != null)
                {
                    continue;
                }

                pawnJacketHediff =
                    HediffMaker.MakeHediff(StraitjacketDefOf.ROM_RestainedByStraitjacket, p);
                p.health.AddHediff(pawnJacketHediff);
            }

            if (jacketOn)
            {
                continue;
            }

            pawnJacketHediff =
                p.health.hediffSet.GetFirstHediffOfDef(StraitjacketDefOf.ROM_RestainedByStraitjacket);
            if (pawnJacketHediff != null)
            {
                p.health.RemoveHediff(pawnJacketHediff);
            }
        }

        //Check our colonists
        foreach (var p in colonists)
        {
            if (p.apparel == null)
            {
                continue;
            }

            var jacketOn = false;

            foreach (var apparel in p.apparel.WornApparel)
            {
                if (apparel.def != StraitjacketDefOf.ROM_Straitjacket)
                {
                    continue;
                }

                straightjackedPawn = p;
                p.needs.mood.thoughts.memories.TryGainMemory(StraitjacketDefOf.ROM_WoreStraitjacket);
                jacketOn = true;

                if (pawnJacketHediff == null)
                {
                    pawnJacketHediff =
                        HediffMaker.MakeHediff(StraitjacketDefOf.ROM_RestainedByStraitjacket, p);
                    p.health.AddHediff(pawnJacketHediff);
                }

                giveThoughtToAll = true; //Different than prisoners
            }

            if (jacketOn)
            {
                continue;
            }

            pawnJacketHediff =
                p.health.hediffSet.GetFirstHediffOfDef(StraitjacketDefOf.ROM_RestainedByStraitjacket);
            if (pawnJacketHediff != null)
            {
                p.health.RemoveHediff(pawnJacketHediff);
            }
        }

        if (!giveThoughtToAll)
        {
            return;
        }

        foreach (var p in colonists)
        {
            if (p != straightjackedPawn)
            {
                p.needs.mood.thoughts.memories.TryGainMemory(StraitjacketDefOf
                    .ROM_ColonistWoreStraitjacket);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lazyTick, "lazyTick", 750);
    }
}