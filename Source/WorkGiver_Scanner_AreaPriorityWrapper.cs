using System;
using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace WorkAreaPriorityManager
{
	public class WorkGiver_Scanner_AreaPriorityWrapper : WorkGiver_Scanner
	{
		private readonly float PRIORITY_SCALE = 1000f;
		public WorkGiver_Scanner wrappedGiver;
		public WorkAreaPrioritization prioritization;

		public WorkGiver_Scanner_AreaPriorityWrapper (WorkGiver_Scanner workGiver, Map map)
		{
			this.wrappedGiver = workGiver;
			this.prioritization = map.GetComponent<AreaPriorityManager>().Prioritizations[workGiver.def];
			this.def = wrappedGiver.def;
		}

		public override bool AllowUnreachable {
			get {
				return wrappedGiver.AllowUnreachable;
			}
		}

		public override int LocalRegionsToScanFirst {
			get {
				return wrappedGiver.LocalRegionsToScanFirst;
			}
		}

		public override PathEndMode PathEndMode {
			get {
				return wrappedGiver.PathEndMode;
			}
		}

		public override ThingRequest PotentialWorkThingRequest {
			get {
				return wrappedGiver.PotentialWorkThingRequest;
			}
		}

		public override bool Prioritized {
			get {
				return true;
			}
		}

		//Low priority 0-1, Normal is 1-1000, High is greater
		public override float GetPriority (Pawn pawn, TargetInfo t)
		{
			WorkAreaPrioritizationType priorityType = this.prioritization.GetWorkAreaPriorityFor(t.Cell);
			if(wrappedGiver.Prioritized)
			{
				float originalPriority = wrappedGiver.GetPriority (pawn, t);
				switch (priorityType) {
				case WorkAreaPrioritizationType.Low:
					return originalPriority / PRIORITY_SCALE;	//Unless original priority is huge this should work
				case WorkAreaPrioritizationType.None:
					return originalPriority;
				case WorkAreaPrioritizationType.High:
					return originalPriority * PRIORITY_SCALE;
				}
			}
			switch (priorityType) {
			case WorkAreaPrioritizationType.Low:
				return 0.5f;
			case WorkAreaPrioritizationType.None:
				return 1f;
			case WorkAreaPrioritizationType.High:
				return PRIORITY_SCALE;
			}
			Log.ErrorOnce("GetPriority was called for WorkGiver_Scanner_AreaPriorityWrapper with a Priority of Avoid ... this should not happen.",
			             48200696);
			return -1f;
		}
        
		public override bool HasJobOnCell (Pawn pawn, IntVec3 c){
		//	if (prioritization.ShouldAvoid (c))
		//		return false;
			return wrappedGiver.HasJobOnCell (pawn, c);
		}

		public override bool HasJobOnThing (Pawn pawn, Thing t, bool forced = false) {
		//	if (prioritization.ShouldAvoid (t.PositionHeld))
		//		return false;
			return wrappedGiver.HasJobOnThing(pawn, t, forced);
		}

		public override Job JobOnCell (Pawn pawn, IntVec3 cell) => wrappedGiver.JobOnCell(pawn, cell);

		public override Job JobOnThing (Pawn pawn, Thing t, bool forced = false) => wrappedGiver.JobOnThing(pawn, t, forced);

		public override Danger MaxPathDanger (Pawn pawn) => wrappedGiver.MaxPathDanger(pawn);
        
        //Shouldn't be needed
		public override Job NonScanJob(Pawn pawn) => wrappedGiver.NonScanJob(pawn);

		public override bool ShouldSkip(Pawn pawn) => wrappedGiver.ShouldSkip(pawn);

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			if (prioritization.avoidPriorityArea == null)
				return wrappedGiver.PotentialWorkCellsGlobal(pawn);
			if (prioritization.invertAvoidArea)
				return wrappedGiver.PotentialWorkCellsGlobal(pawn).Intersect(prioritization.avoidPriorityArea.ActiveCells);
			return wrappedGiver.PotentialWorkCellsGlobal(pawn).Except(prioritization.avoidPriorityArea.ActiveCells);
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			if (prioritization.avoidPriorityArea == null)
			    return wrappedGiver.PotentialWorkThingsGlobal(pawn);
			if (prioritization.invertAvoidArea)
				return wrappedGiver.PotentialWorkThingsGlobal(pawn).Where(t => prioritization.avoidPriorityArea[t.PositionHeld]);
			return wrappedGiver.PotentialWorkThingsGlobal(pawn).Where(t => !prioritization.avoidPriorityArea[t.PositionHeld]);
		}
	}
}

