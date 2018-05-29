using System;
using Verse;
using System.Collections.Generic;
using System.Collections;

namespace WorkAreaPriorityManager
{
	public class WorkAreaPrioritization : IExposable
	{
		public Area highPriorityArea;
		public Area lowPriorityArea;
		public Area avoidPriorityArea;

		public bool disabled = false;
		public bool invertHighArea = false;
		public bool invertLowArea = false;
		public bool invertAvoidArea = false;

		public IEnumerable<Area> Areas {
			get {
				if (highPriorityArea != null)
					yield return highPriorityArea;
				if (lowPriorityArea != null)
					yield return lowPriorityArea;
				if (avoidPriorityArea != null)
					yield return avoidPriorityArea;
			}
		}

		public bool ShouldAvoid(IntVec3 cell)
		{
			if (avoidPriorityArea == null)
				return false;
			return (!invertAvoidArea && avoidPriorityArea [cell]) || (invertAvoidArea && !avoidPriorityArea [cell]);
		}
			
		public void ExposeData()
		{
			Scribe_References.Look<Area> (ref this.highPriorityArea, "HighPriorityArea");
			Scribe_References.Look<Area> (ref this.lowPriorityArea, "LowPriorityArea");
			Scribe_References.Look<Area> (ref this.avoidPriorityArea, "AvoidPriorityArea");
			Scribe_Values.Look<bool> (ref this.disabled, "Disabled");
			Scribe_Values.Look<bool> (ref this.invertHighArea, "InvertHighArea");
			Scribe_Values.Look<bool> (ref this.invertLowArea, "InvertLowArea");
			Scribe_Values.Look<bool> (ref this.invertAvoidArea, "InvertAvoidArea");
		}

		public WorkAreaPrioritizationType GetWorkAreaPriorityFor(IntVec3 cell)
		{
			if (avoidPriorityArea != null &&
				((!invertAvoidArea && avoidPriorityArea [cell]) || (invertAvoidArea && !avoidPriorityArea[cell])))
				return WorkAreaPrioritizationType.Avoid;
			if (highPriorityArea != null &&
				((!invertHighArea && highPriorityArea [cell]) || (invertHighArea && !highPriorityArea[cell])))
				return WorkAreaPrioritizationType.High;
			if (lowPriorityArea != null && 
				((!invertLowArea && lowPriorityArea [cell]) || (invertLowArea && !lowPriorityArea[cell])))
				return WorkAreaPrioritizationType.Low;
			return WorkAreaPrioritizationType.None;
		}
	}
}

