using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace WorkAreaPriorityManager
{
	public enum WorkAreaPrioritizationType { None, High, Low, Avoid };

	public class AreaPriorityManager : MapComponent
	{
		private Dictionary<WorkGiverDef, WorkAreaPrioritization> prioritizations;
		private List<WorkGiverDef> exposeHelper1;
		private List<WorkAreaPrioritization> exposeHelper2;

		public Dictionary<WorkGiverDef, WorkAreaPrioritization> Prioritizations {
			get {
				return this.prioritizations;
			}
		}

		public AreaPriorityManager (Map map) : base(map)
		{
			this.prioritizations = new Dictionary<WorkGiverDef, WorkAreaPrioritization> (DefDatabase<WorkTypeDef>.DefCount);
			foreach (var workTypeDef in DefDatabase<WorkGiverDef>.AllDefsListForReading)
				this.prioritizations.Add (workTypeDef, null);
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<WorkGiverDef, WorkAreaPrioritization> (ref this.prioritizations, "Prioritizations", 
				LookMode.Def, LookMode.Deep, ref this.exposeHelper1, ref this.exposeHelper2);
		}

		public void LaunchDialog_ManageWorkAreaPriorities()
		{
			var dialog = new Dialog_ManageWorkAreaPriorities (this.map);
			Find.WindowStack.Add (dialog);
		}
	}
}
