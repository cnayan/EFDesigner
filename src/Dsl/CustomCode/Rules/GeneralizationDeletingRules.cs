﻿using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Modeling;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [RuleOn(typeof(Generalization), FireTime = TimeToFire.TopLevelCommit)]
   internal class GeneralizationDeletingRules : DeletingRule
   {
      /// <inheritdoc />
      public override void ElementDeleting(ElementDeletingEventArgs e)
      {
         base.ElementDeleting(e);

         Generalization element = (Generalization)e.ModelElement;
         Store store = element.Store;
         Transaction current = store.TransactionManager.CurrentTransaction;

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         if (element.Superclass.IsDeleting)
            return;

         ModelClass superclass = element.Superclass;
         ModelClass subclass = element.Subclass;
         List<Association> associations = store.Get<Association>().Where(a => a.Source == superclass || a.Target == superclass).ToList();

         if (!superclass.AllAttributes.Any() && !associations.Any())
            return;

         if (!subclass.IsDeleting && BooleanQuestionDisplay.Show($"Push {superclass.Name} attributes and associations down to {subclass.Name}?") == true)
            superclass.PushDown(subclass);
      }
   }
}
