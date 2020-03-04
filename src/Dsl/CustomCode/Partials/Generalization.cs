﻿using System.Collections.Generic;

namespace Sawczyn.EFDesigner.EFModel
{
   public partial class Generalization: IHasStore
   {
      public bool IsInCircularInheritance()
      {
         List<ModelClass> classes = new List<ModelClass>();
         for (ModelClass modelClass = Subclass; modelClass != null; modelClass = modelClass.Superclass)
         {
            if (classes.Contains(modelClass))
               return true;

            classes.Add(modelClass);
         }

         return false;
      }

      public string GetDisplayText()
      {
         return $"{Subclass.Name} inherits from {Superclass.Name}";
      }

      private string GetNameValue()
      {
         return GetDisplayText();
      }

   }
}
