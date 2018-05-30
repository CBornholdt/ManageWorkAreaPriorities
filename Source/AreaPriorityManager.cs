using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		IEnumerable<WorkGiverDef> PrioritizableWorkGivers {
			get {
				foreach (var def in DefDatabase<WorkGiverDef>.AllDefsListForReading) 
				/*	if (!def.modExtensions.NullOrEmpty()) {
						Log.Message(def.defName + " HIT");
						foreach (var mod in def.modExtensions)
							foreach (var property in mod.GetType().GetProperties)
								Log.Message(def.defName + " " + field.Name + " " + field.FieldType.Name);
					} else
						yield return def;   */
					if (def.modExtensions.NullOrEmpty() || !def.modExtensions
					    .Any(ext => ((string)ext.GetType().GetProperty("Tag").GetValue(ext, new object[0]) == "Dynamic")))
						yield return def;   
			}
		}

		public AreaPriorityManager (Map map) : base(map)
		{
			this.prioritizations = new Dictionary<WorkGiverDef, WorkAreaPrioritization> (DefDatabase<WorkGiverDef>.DefCount);
			foreach (var workTypeDef in PrioritizableWorkGivers)
				this.prioritizations.Add (workTypeDef, null);
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<WorkGiverDef, WorkAreaPrioritization> (ref this.prioritizations, "Prioritizations", 
				LookMode.Def, LookMode.Deep, ref this.exposeHelper1, ref this.exposeHelper2);

			if (Scribe.mode == LoadSaveMode.PostLoadInit)   //If a mod was loaded that adds new work givers ...
				foreach (var newWorkGiver in PrioritizableWorkGivers.Except(this.prioritizations.Keys))
					this.prioritizations.Add(newWorkGiver, null);
		}

		public void LaunchDialog_ManageWorkAreaPriorities()
		{
			var dialog = new Dialog_ManageWorkAreaPriorities (this.map);
			Find.WindowStack.Add (dialog);
		}
	}
}
