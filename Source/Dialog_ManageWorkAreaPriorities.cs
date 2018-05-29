using System;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace WorkAreaPriorityManager
{
	public class Dialog_ManageWorkAreaPriorities : Window
	{
		private readonly Map map;

		private readonly float footerHeight = 150f;
		private Vector2 scrollPosition = Vector2.zero;
		private float viewHeight = 800f;
		public static readonly float cellSpacing = 2f;

		public Map Map {
			get { return this.map; }
		}

		public override Vector2 InitialSize {
			get { return new Vector2 (700, 700); }
		}

		public float LineHeight {
			get { return Text.LineHeight + CellSpacing; }
		}

		public float CellSpacing {
			get { return cellSpacing; }
		}

		public float RowSpacing {
			get { return LineHeight * 0.75f; }
		}

		public float RowHeight {
			get { return 2f * cellSpacing + 3f * LineHeight; }
		}

		public Dialog_ManageWorkAreaPriorities (Map map)
		{
			this.map = map;
			this.forcePause = true;
			this.doCloseX = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
		}

		private float DoWorkAreaPriorityRowElement(WorkGiverDef giverType, Rect inRect)
		{
			Rect rect1 = inRect.ContractedBy (cellSpacing);	//usedHeight considered in return
			Rect rect2 = new Rect (rect1.xMin, rect1.yMin, rect1.width * 0.35f, rect1.height);
			Rect rect3 = new Rect (rect2.xMax + rect1.width * 0.1f, rect2.yMin, rect1.width * 0.55f, rect1.height);

			AreaPriorityManager aPM = map.GetComponent<AreaPriorityManager> ();
			WorkAreaPrioritization wAP = aPM.Prioritizations [giverType];
            
			Rect rect2a = new Rect (rect2.xMin + 2 * CellSpacing, rect2.yMin, rect2.width - 4 * CellSpacing, LineHeight);
			Rect rect2b = new Rect (rect2a.xMin + 4 * CellSpacing, rect2a.yMax, rect2.width / 2, LineHeight);
			Rect rect2c = new Rect (rect2b.xMin, rect2b.yMax, rect2b.width, LineHeight);
			Widgets.Label (rect2a, giverType.workType.gerundLabel.CapitalizeFirst() + ": " 
			               + giverType.gerund.CapitalizeFirst());
			Widgets.CheckboxLabeled (rect2b, "Disable".Translate(), ref wAP.disabled);
			Widgets.Label (rect2c, "Remove".Translate ());
			if (Widgets.CloseButtonFor (rect2c))
				aPM.Prioritizations [giverType] = null;

			WidgetRow rowA = new WidgetRow (rect3.xMin, rect3.yMin, UIDirection.RightThenUp, rect3.width, CellSpacing * 2f);
			WidgetRow rowB = new WidgetRow (rect3.xMin, rect3.yMin + LineHeight, UIDirection.RightThenUp, rect3.width, CellSpacing * 2f);
			WidgetRow rowC = new WidgetRow (rect3.xMin, rect3.yMin + 2 * LineHeight, UIDirection.RightThenUp, rect3.width, CellSpacing * 2f);

			if(rowA.ButtonText("HighPriority".Translate() + ": " + (wAP.highPriorityArea?.Label ?? "None"))){
				List<FloatMenuOption> highPriorityChoices = new List<FloatMenuOption>();
				foreach(Area area in map.areaManager.AllAreas.Except(wAP.Areas)) 
					highPriorityChoices.Add(new FloatMenuOption(area.Label, () => wAP.highPriorityArea = area));
				if (wAP.highPriorityArea != null)
					highPriorityChoices.Add(new FloatMenuOption("None".Translate(), () => wAP.highPriorityArea = null));
				Find.WindowStack.Add(new FloatMenu(highPriorityChoices));
			}
			float gapAdjust = (rect3.xMin + 0.7f * rect3.width) - rowA.FinalX;	//Start invert box 70% across dialog
			if(gapAdjust > 0)
				rowA.Gap(gapAdjust);
			if(rowA.ButtonText(wAP.invertHighArea ? "Inverted".Translate() : "NotInverted".Translate()))
				wAP.invertHighArea = !wAP.invertHighArea;

			if(rowB.ButtonText("LowPriority".Translate() + ": " + (wAP.lowPriorityArea?.Label ?? "None"))){
				List<FloatMenuOption> lowPriorityChoices = new List<FloatMenuOption>();
				foreach(Area area in map.areaManager.AllAreas.Except(wAP.Areas)) 
					lowPriorityChoices.Add(new FloatMenuOption(area.Label, () => wAP.lowPriorityArea = area));
				if (wAP.lowPriorityArea != null)
					lowPriorityChoices.Add(new FloatMenuOption("None".Translate(), () => wAP.lowPriorityArea = null));
				Find.WindowStack.Add(new FloatMenu(lowPriorityChoices));
			}
			gapAdjust = (rect3.xMin + 0.7f * rect3.width) - rowB.FinalX;	
			if(gapAdjust > 0)
				rowB.Gap(gapAdjust);
			if(rowB.ButtonText(wAP.invertLowArea ? "Inverted".Translate() : "NotInverted".Translate()))
				wAP.invertLowArea = !wAP.invertLowArea;

			if(rowC.ButtonText("AvoidPriority".Translate() + ": " + (wAP.avoidPriorityArea?.Label ?? "None"))){
				List<FloatMenuOption> avoidPriorityChoices = new List<FloatMenuOption>();
				foreach(Area area in map.areaManager.AllAreas.Except(wAP.Areas)) 
					avoidPriorityChoices.Add(new FloatMenuOption(area.Label, () => wAP.avoidPriorityArea = area));
				if (wAP.avoidPriorityArea != null)
					avoidPriorityChoices.Add(new FloatMenuOption("None".Translate(), () => wAP.avoidPriorityArea = null));
				Find.WindowStack.Add(new FloatMenu(avoidPriorityChoices));
			}
			gapAdjust = (rect3.xMin + 0.7f * rect3.width) - rowC.FinalX;	
			if(gapAdjust > 0)
				rowC.Gap(gapAdjust);
			if(rowC.ButtonText(wAP.invertAvoidArea ? "Inverted".Translate() : "NotInverted".Translate()))
				wAP.invertAvoidArea = !wAP.invertAvoidArea;

			inRect.height = 3f * LineHeight + CellSpacing;
			Widgets.DrawBox(inRect, 1);
				
			return RowHeight;
		}

		private void DoFooterContents(Rect inRect)
		{
			AreaPriorityManager wAPM = map.GetComponent<AreaPriorityManager> ();

			Action<WorkTypeDef> createWorkCategorySubMenu = (WorkTypeDef workType) => {
				List<FloatMenuOption> subMenuList = new List<FloatMenuOption> ();
				foreach (var giver in DefDatabase<WorkGiverDef>.AllDefsListForReading.Where(def => def.workType == workType))
					if (wAPM.Prioritizations [giver] == null)
						subMenuList.Add (new FloatMenuOption ("Prioritize".Translate () + ": " + giver.gerund.CapitalizeFirst()
							, () => wAPM.Prioritizations [giver] = new WorkAreaPrioritization ()));
				Find.WindowStack.Add (new FloatMenu (subMenuList));
			};

			Listing_Standard listing = new Listing_Standard ();
			listing.Begin (inRect);
			listing.ColumnWidth = inRect.width / 2;
			
			var availableWorkTypes = wAPM.Prioritizations.Keys
				.Where (key => wAPM.Prioritizations [key] == null)
				.Select(wg => wg.workType)
				.Distinct();

			if (listing.ButtonText ("NewWorkAreaPrioritization".Translate ()) && availableWorkTypes.Any()) {
				List<FloatMenuOption> newWAPList = new List<FloatMenuOption> ();
				foreach (WorkTypeDef workType in availableWorkTypes)
					newWAPList.Add (new FloatMenuOption ("Prioritize".Translate () + ": " + workType.gerundLabel.CapitalizeFirst()
						, () => createWorkCategorySubMenu(workType)));
				Find.WindowStack.Add (new FloatMenu (newWAPList));
			}
			var prioritizedWork = wAPM.Prioritizations.Keys.Where (key => wAPM.Prioritizations [key] != null);
			if (listing.ButtonText ("RemoveWorkAreaPrioritization".Translate ()) && prioritizedWork.Any()) {
				List<FloatMenuOption> removeWAPList = new List<FloatMenuOption> ();
				foreach (WorkGiverDef giverType in prioritizedWork)
					removeWAPList.Add (new FloatMenuOption ("Remove".Translate () + ": " + giverType.gerund.CapitalizeFirst()
						, () => wAPM.Prioritizations[giverType] = null));
				Find.WindowStack.Add (new FloatMenu (removeWAPList));
			}
			listing.NewColumn ();
			if (listing.ButtonText("ManageAreas".Translate())) {
				Find.WindowStack.Add(new Dialog_ManageAreas(map));
				Close();
			}

			if (LoadedModManager.RunningMods.Any(mod => mod.Name == "Composite Area Manager") &&
			    listing.ButtonText("ManageCompositeAreas".Translate())) {
                MethodInfo getComponent = typeof(Map).GetMethod("GetComponent", new Type[1] { typeof(Type) });
                Type wAPMType = Type.GetType("CompositeAreaManager.CompositeAreaManager, CompositeAreaManager");
                MethodInfo launchWAPMDialog = wAPMType.GetMethod("LaunchDialog_ManageCompositeAreas");
                launchWAPMDialog.Invoke(getComponent.Invoke(map, new object[1] { wAPMType }), new object[0]);
                Close();
            }

			listing.End ();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect scrollRect = new Rect (inRect).ContractedBy(CellSpacing);
			scrollRect.height -= this.footerHeight;
			Rect scrollViewRect = new Rect (0, 0, scrollRect.width - 2 * GUI.skin.verticalScrollbar.fixedWidth, this.viewHeight);
			Widgets.BeginScrollView (scrollRect, ref this.scrollPosition, scrollViewRect, true);
			var prioritizedWork = map.GetComponent<AreaPriorityManager>().Prioritizations.Keys
				.Where (key => map.GetComponent<AreaPriorityManager>().Prioritizations [key] != null).ToList();
			Rect rowDrawRect = new Rect (scrollViewRect);
			for(int i = prioritizedWork.Count; i-- > 0;)
				rowDrawRect.yMin += DoWorkAreaPriorityRowElement (prioritizedWork[i], rowDrawRect) + RowSpacing;
			viewHeight = rowDrawRect.y + 250;	//Issues with nested listings resizing if not enough space

			Widgets.EndScrollView ();
			Rect footerRect = new Rect (inRect);
			footerRect.yMin = footerRect.yMax - this.footerHeight;
			this.DoFooterContents (footerRect); 
		}
	}
}

