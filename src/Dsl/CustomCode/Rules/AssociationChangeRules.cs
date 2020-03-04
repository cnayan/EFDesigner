﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Immutability;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <inheritdoc />
   [RuleOn(typeof(Association), FireTime = TimeToFire.TopLevelCommit)]
   public class AssociationChangeRules : ChangeRule
   {
      // matches [Display(Name="*")] and [System.ComponentModel.DataAnnotations.Display(Name="*")]
      private static readonly Regex DisplayAttributeRegex = new Regex("^(.*)\\[(System\\.ComponentModel\\.DataAnnotations\\.)?Display\\(Name=\"([^\"]+)\"\\)\\](.*)$", RegexOptions.Compiled);

      private static void CheckSourceForDisplayText(BidirectionalAssociation bidirectionalAssociation)
      {
         Match match = DisplayAttributeRegex.Match(bidirectionalAssociation.SourceCustomAttributes);

         // is there a custom attribute for [Display]?
         if (match != Match.Empty)
         {
            // if SourceDisplayText is empty, move the Name down to SourceDisplayText
            if (string.IsNullOrWhiteSpace(bidirectionalAssociation.SourceDisplayText))
               bidirectionalAssociation.SourceDisplayText = match.Groups[3].Value;

            // if custom attribute's Name matches SourceDisplayText, remove that attribute, leaving other custom attributes if present
            if (match.Groups[3].Value == bidirectionalAssociation.SourceDisplayText)
               bidirectionalAssociation.SourceCustomAttributes = match.Groups[1].Value + match.Groups[4].Value;
         }
      }

      private static void CheckTargetForDisplayText(Association association)
      {
         Match match = DisplayAttributeRegex.Match(association.TargetCustomAttributes);

         // is there a custom attribute for [Display]?
         if (match != Match.Empty)
         {
            // if TargetDisplayText is empty, move the Name down to TargetDisplayText
            if (string.IsNullOrWhiteSpace(association.TargetDisplayText))
               association.TargetDisplayText = match.Groups[3].Value;

            // if custom attribute's Name matches TargetDisplayText, remove that attribute, leaving other custom attributes if present
            if (match.Groups[3].Value == association.TargetDisplayText)
               association.TargetCustomAttributes = match.Groups[1].Value + match.Groups[4].Value;
         }
      }

      /// <inheritdoc />
      public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
      {
         base.ElementPropertyChanged(e);

         Association element = (Association)e.ModelElement;
         if (element.IsDeleted)
            return;

         Store store = element.Store;
         Transaction current = store.TransactionManager.CurrentTransaction;

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         if (Equals(e.NewValue, e.OldValue))
            return;

         List<string> errorMessages = EFCoreValidator.GetErrors(element).ToList();
         BidirectionalAssociation bidirectionalAssociation = element as BidirectionalAssociation;

         using (Transaction inner = store.TransactionManager.BeginTransaction("Redraw Association"))
         {
            switch (e.DomainProperty.Name)
            {
               case "FKPropertyName":
               {
                  string fkPropertyName = e.NewValue?.ToString();
                  bool fkPropertyError = false;

                  // these can be multiples, separated by a comma
                  string[] priorForeignKeyPropertyNames = e.OldValue?.ToString().Split(',').Select(n => n.Trim()).ToArray() ?? new string[0];

                  IEnumerable<ModelAttribute> priorForeignKeyModelAttributes = string.IsNullOrEmpty(e.OldValue?.ToString())
                                                                                     ? Array.Empty<ModelAttribute>()
                                                                                     : priorForeignKeyPropertyNames
                                                                                      .Select(oldValue => element.Dependent.Attributes.FirstOrDefault(a => a.Name == oldValue))
                                                                                      .ToArray();

                  if (!string.IsNullOrEmpty(fkPropertyName))
                  {
                     string tag = $"({element.Source.Name}:{element.Target.Name})";

                     if (element.Dependent == null)
                     {
                        errorMessages.Add($"{tag} can't have foreign keys defined; no dependent role found");

                        break;
                     }

                     string[] foreignKeyPropertyNames = element.GetForeignKeyPropertyNames();
                     int propertyCount = foreignKeyPropertyNames.Length;
                     int identityCount = element.Principal.AllIdentityAttributes.Count();

                     if (propertyCount != identityCount)
                     {
                        errorMessages.Add(
                                          $"{tag} foreign key must have zero or {identityCount} {(identityCount == 1 ? "property" : "properties")} defined, since "
                                        + $"{element.Principal.Name} has {identityCount} identity properties; found {propertyCount} instead");

                        fkPropertyError = true;
                     }

                     // validate names
                     foreach (string propertyName in foreignKeyPropertyNames)
                     {
                        if (!CodeGenerator.IsValidLanguageIndependentIdentifier(propertyName))
                        {
                           errorMessages.Add($"{tag} FK property name '{propertyName}' isn't a valid .NET identifier");
                           fkPropertyError = true;
                        }

                        if (element.Dependent.AllAttributes.Except(element.Dependent.Attributes).Any(a => a.Name == propertyName))
                        {
                           errorMessages.Add($"{tag} FK property name '{propertyName}' is used in a base class of {element.Dependent.Name}");
                           fkPropertyError = true;
                        }
                     }

                     if (!fkPropertyError)
                     {
                        // remove any flags and locks on the attributes that were foreign keys
                        foreach (ModelAttribute modelAttribute in priorForeignKeyModelAttributes)
                        {
                           modelAttribute.SetLocks(Locks.None);
                           modelAttribute.Summary = null;
                           modelAttribute.IsForeignKey = false;
                           modelAttribute.RedrawItem();
                        }

                        element.EnsureForeignKeyAttributes();

                        IEnumerable<ModelAttribute> currentForeignKeyModelAttributes = foreignKeyPropertyNames.Select(newValue => element.Dependent.Attributes.FirstOrDefault(a => a.Name == newValue));

                        // add delete flags and locks to the attributes that are now foreign keys
                        foreach (ModelAttribute modelAttribute in currentForeignKeyModelAttributes)
                        {
                           modelAttribute.SetLocks(Locks.None);
                           modelAttribute.Summary = $"Foreign key for {element.GetDisplayText()}. {modelAttribute.Summary}";
                           modelAttribute.SetLocks(Locks.Delete);
                           modelAttribute.IsForeignKey = true;
                           modelAttribute.RedrawItem();
                        }
                     }
                  }
                  else
                  {
                     // foreign key was removed
                     // remove locks
                     foreach (ModelAttribute modelAttribute in priorForeignKeyModelAttributes)
                     {
                        modelAttribute.SetLocks(Locks.None);
                        modelAttribute.IsForeignKey = false;

                        string addedSummaryText = $"Foreign key for {element.GetDisplayText()}. ";

                        modelAttribute.Summary = modelAttribute.Summary.Length >= addedSummaryText.Length
                                                       ? modelAttribute.Summary.Substring(addedSummaryText.Length)
                                                       : null;

                        modelAttribute.RedrawItem();
                     }
                  }

               }

                  break;

               case "SourceCustomAttributes":

                  if (bidirectionalAssociation != null && !string.IsNullOrWhiteSpace(bidirectionalAssociation.SourceCustomAttributes))
                  {
                     bidirectionalAssociation.SourceCustomAttributes = $"[{bidirectionalAssociation.SourceCustomAttributes.Trim('[', ']')}]";
                     CheckSourceForDisplayText(bidirectionalAssociation);
                  }

                  break;

               case "SourceDisplayText":

                  if (bidirectionalAssociation != null)
                     CheckSourceForDisplayText(bidirectionalAssociation);

                  break;

               case "SourceMultiplicity":
                  Multiplicity sourceMultiplicity = (Multiplicity)e.NewValue;

                  // change unidirectional source cardinality
                  // if target is dependent
                  //    source cardinality is 0..1 or 1
                  if (element.Target.IsDependentType && sourceMultiplicity == Multiplicity.ZeroMany)
                  {
                     errorMessages.Add($"Can't have a 0..* association from {element.Target.Name} to dependent type {element.Source.Name}");

                     break;
                  }

                  if ((sourceMultiplicity == Multiplicity.One && element.TargetMultiplicity == Multiplicity.One)
                   || (sourceMultiplicity == Multiplicity.ZeroOne && element.TargetMultiplicity == Multiplicity.ZeroOne))
                  {
                     if (element.SourceRole != EndpointRole.NotSet)
                        element.SourceRole = EndpointRole.NotSet;

                     if (element.TargetRole != EndpointRole.NotSet)
                        element.TargetRole = EndpointRole.NotSet;
                  }
                  else
                     SetEndpointRoles(element);

                  // cascade delete behavior could now be illegal. Reset to default
                  element.SourceDeleteAction = DeleteAction.Default;
                  element.TargetDeleteAction = DeleteAction.Default;

                  break;

               case "SourcePropertyName":
                  string sourcePropertyNameErrorMessage = ValidateAssociationIdentifier(element, element.Target, (string)e.NewValue);

                  if (EFModelDiagram.IsDropping && sourcePropertyNameErrorMessage != null)
                     element.Delete();
                  else
                     errorMessages.Add(sourcePropertyNameErrorMessage);

                  break;

               case "SourceRole":

                  if (element.Source.IsDependentType)
                  {
                     element.SourceRole = EndpointRole.Dependent;
                     element.TargetRole = EndpointRole.Principal;
                  }
                  else if (!SetEndpointRoles(element))
                  {
                     if (element.SourceRole == EndpointRole.Dependent && element.TargetRole != EndpointRole.Principal)
                        element.TargetRole = EndpointRole.Principal;
                     else if (element.SourceRole == EndpointRole.Principal && element.TargetRole != EndpointRole.Dependent)
                        element.TargetRole = EndpointRole.Dependent;
                  }

                  break;

               case "TargetCustomAttributes":

                  if (!string.IsNullOrWhiteSpace(element.TargetCustomAttributes))
                  {
                     element.TargetCustomAttributes = $"[{element.TargetCustomAttributes.Trim('[', ']')}]";
                     CheckTargetForDisplayText(element);
                  }

                  break;

               case "TargetDisplayText":

                  CheckTargetForDisplayText(element);

                  break;

               case "TargetMultiplicity":
                  Multiplicity newTargetMultiplicity = (Multiplicity)e.NewValue;

                  // change unidirectional target cardinality
                  // if target is dependent
                  //    target cardinality must be 0..1 or 1
                  if (element.Target.IsDependentType && newTargetMultiplicity == Multiplicity.ZeroMany)
                  {
                     errorMessages.Add($"Can't have a 0..* association from {element.Source.Name} to dependent type {element.Target.Name}");

                     break;
                  }

                  if ((element.SourceMultiplicity == Multiplicity.One && newTargetMultiplicity == Multiplicity.One)
                   || (element.SourceMultiplicity == Multiplicity.ZeroOne && newTargetMultiplicity == Multiplicity.ZeroOne))
                  {
                     if (element.SourceRole != EndpointRole.NotSet)
                        element.SourceRole = EndpointRole.NotSet;

                     if (element.TargetRole != EndpointRole.NotSet)
                        element.TargetRole = EndpointRole.NotSet;
                  }
                  else
                     SetEndpointRoles(element);

                  // cascade delete behavior could now be illegal. Reset to default
                  element.SourceDeleteAction = DeleteAction.Default;
                  element.TargetDeleteAction = DeleteAction.Default;

                  break;

               case "TargetPropertyName":

                  // if we're creating an association via drag/drop, it's possible the existing property name
                  // is the same as the default property name. The default doesn't get created until the transaction is 
                  // committed, so the drop's action will cause a name clash. Remove the clashing property, but
                  // only if drag/drop.

                  string targetPropertyNameErrorMessage = ValidateAssociationIdentifier(element, element.Source, (string)e.NewValue);

                  if (EFModelDiagram.IsDropping && targetPropertyNameErrorMessage != null)
                     element.Delete();
                  else
                     errorMessages.Add(targetPropertyNameErrorMessage);

                  break;

               case "TargetRole":

                  if (element.Target.IsDependentType)
                  {
                     element.SourceRole = EndpointRole.Principal;
                     element.TargetRole = EndpointRole.Dependent;
                  }
                  else if (!SetEndpointRoles(element))
                  {
                     if (element.TargetRole == EndpointRole.Dependent && element.SourceRole != EndpointRole.Principal)
                        element.SourceRole = EndpointRole.Principal;
                     else if (element.TargetRole == EndpointRole.Principal && element.SourceRole != EndpointRole.Dependent)
                        element.SourceRole = EndpointRole.Dependent;
                  }

                  break;
            }


            element.RedrawItem();
            inner.Commit();
         }

         errorMessages = errorMessages.Where(m => m != null).ToList();

         if (errorMessages.Any())
         {
            current.Rollback();
            ErrorDisplay.Show(string.Join("\n", errorMessages));
         }
      }

      internal static bool SetEndpointRoles(Association element)
      {
         switch (element.TargetMultiplicity)
         {
            case Multiplicity.ZeroMany:

               switch (element.SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     element.SourceRole = EndpointRole.NotApplicable;
                     element.TargetRole = EndpointRole.NotApplicable;

                     return true;
                  case Multiplicity.One:
                     element.SourceRole = EndpointRole.Principal;
                     element.TargetRole = EndpointRole.Dependent;

                     return true;
                  case Multiplicity.ZeroOne:
                     element.SourceRole = EndpointRole.Principal;
                     element.TargetRole = EndpointRole.Dependent;

                     return true;
               }

               break;
            case Multiplicity.One:

               switch (element.SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     element.SourceRole = EndpointRole.Dependent;
                     element.TargetRole = EndpointRole.Principal;

                     return true;
                  case Multiplicity.One:

                     return false;
                  case Multiplicity.ZeroOne:
                     element.SourceRole = EndpointRole.Dependent;
                     element.TargetRole = EndpointRole.Principal;

                     return true;
               }

               break;
            case Multiplicity.ZeroOne:

               switch (element.SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     element.SourceRole = EndpointRole.Dependent;
                     element.TargetRole = EndpointRole.Principal;

                     return true;
                  case Multiplicity.One:
                     element.SourceRole = EndpointRole.Principal;
                     element.TargetRole = EndpointRole.Dependent;

                     return true;
                  case Multiplicity.ZeroOne:

                     return false;
               }

               break;
         }

         return false;
      }

      private static string ValidateAssociationIdentifier(Association association, ModelClass targetedClass, string identifier)
      {
         if (string.IsNullOrWhiteSpace(identifier) || !CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
            return $"{identifier} isn't a valid .NET identifier";

         ModelClass offendingModelClass = targetedClass.AllAttributes.FirstOrDefault(x => x.Name == identifier)?.ModelClass ??
                                          targetedClass.AllNavigationProperties(association).FirstOrDefault(x => x.PropertyName == identifier)?.ClassType;

         return offendingModelClass != null
                   ? $"Duplicate symbol {identifier} in {offendingModelClass.Name}"
                   : null;
      }
   }
}
