﻿using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{

   [RuleOn(typeof(EnumShape), FireTime = TimeToFire.TopLevelCommit)]
   internal class EnumShapeAddRules : AddRule
   {
      public override void ElementAdded(ElementAddedEventArgs e)
      {
         base.ElementAdded(e);

         EnumShape element = (EnumShape)e.ModelElement;
         Store store = element.Store;
         Transaction current = store.TransactionManager.CurrentTransaction;
         ModelRoot modelRoot = store.ModelRoot();

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         // if the diagram we're adding to isn't the current visible diagram, add the shape but don't show it
         Diagram currentDiagram = ModelRoot.GetCurrentDiagram?.Invoke();

         if (element.Diagram != currentDiagram)
            element.Hide();
      }
   }

}