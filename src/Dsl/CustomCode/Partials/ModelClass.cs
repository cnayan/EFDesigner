﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Validation;

using Sawczyn.EFDesigner.EFModel.Extensions;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace Sawczyn.EFDesigner.EFModel
{
   [ValidationState(ValidationState.Enabled)]
   public partial class ModelClass : IModelElementWithCompartments, IDisplaysWarning, IHasStore
   {
      [Browsable(false)]
      public string DefaultNamespace
      {
         get
         {
            if (IsDependentType && !string.IsNullOrWhiteSpace(ModelRoot?.StructNamespace))
               return ModelRoot.StructNamespace;

            if (!IsDependentType && !string.IsNullOrWhiteSpace(ModelRoot?.EntityNamespace))
               return ModelRoot.EntityNamespace;

            return ModelRoot?.Namespace;
         }
      }

      /// <summary>
      /// Namespace for generated code. Takes overrides into account.
      /// </summary>
      [Browsable(false)]
      public string EffectiveNamespace
      {
         get
         {
            return namespaceStorage ?? DefaultNamespace;
         }
      }

      [Browsable(false)]
      public string DefaultOutputDirectory
      {
         get
         {
            if (IsDependentType && !string.IsNullOrWhiteSpace(ModelRoot?.StructOutputDirectory))
               return ModelRoot.StructOutputDirectory;

            if (!IsDependentType && !string.IsNullOrWhiteSpace(ModelRoot?.EntityOutputDirectory))
               return ModelRoot.EntityOutputDirectory;

            return ModelRoot?.ContextOutputDirectory;
         }
      }

      /// <summary>
      /// Output directory for generated code. Takes overrides into account.
      /// </summary>
      [Browsable(false)]
      public string EffectiveOutputDirectory
      {
         get
         {
            return outputDirectoryStorage ?? DefaultOutputDirectory;
         }
      }

      public string GetDisplayText() => Name;

      /// <summary>
      /// All attributes in the class, including those inherited from base classes
      /// </summary>
      public IEnumerable<ModelAttribute> AllAttributes
      {
         get
         {
            List<ModelAttribute> result = Attributes.ToList();
            if (Superclass != null)
               result.AddRange(Superclass.AllAttributes);
            return result;
         }
      }

      /// <summary>
      /// Names of all properties in the class
      /// </summary>
      public IEnumerable<string> AllPropertyNames
      {
         get
         {
            List<string> result = AllAttributes.Select(a => a.Name).ToList();

            result.AddRange(AllNavigationProperties().Select(np => np.PropertyName));

            //if (Superclass != null)
            //   result.AddRange(Superclass.AllPropertyNames);

            return result;
         }
      }

      /// <summary>
      /// All required attributes defined in this class
      /// </summary>
      public IEnumerable<ModelAttribute> RequiredAttributes
      {
         get
         {
            return Attributes.Where(x => x.Required).ToList();
         }
      }

      /// <summary>
      /// All required attributes in the inheritance chain
      /// </summary>
      public IEnumerable<ModelAttribute> AllRequiredAttributes
      {
         get
         {
            return AllAttributes.Where(x => x.Required).ToList();
         }
      }

      /// <summary>
      /// All identity attributes defined in this class
      /// </summary>
      public IEnumerable<ModelAttribute> IdentityAttributes
      {
         get
         {
            return Attributes.Where(x => x.IsIdentity).ToList();
         }
      }

      /// <summary>
      /// All identity attributes in the inheritance chain
      /// </summary>
      public IEnumerable<ModelAttribute> AllIdentityAttributes
      {
         get
         {
            return AllAttributes.Where(x => x.IsIdentity).ToList();
         }
      }

      /// <summary>
      /// Names of identity attributes defined in this class
      /// </summary>
      public IEnumerable<string> IdentityAttributeNames
      {
         get
         {
            return IdentityAttributes.Select(x => x.Name).ToList();
         }
      }

      /// <summary>
      /// Names of all identity attributes in the inheritance chain
      /// </summary>
      public IEnumerable<string> AllIdentityAttributeNames
      {
         get
         {
            return AllIdentityAttributes.Select(x => x.Name).ToList();
         }
      }

      /// <summary>
      /// Class name with namespace
      /// </summary>
      public string FullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(EffectiveNamespace)
                      ? $"global::{Name}"
                      : $"global::{EffectiveNamespace}.{Name}";
         }
      }

      #region Warning display

      // set as methods to avoid issues around serialization

      private bool hasWarning;

      /// <summary>
      /// Determines if this class has warnings being displayed.
      /// </summary>
      /// <returns>True if this class has warnings visible, false otherwise</returns>
      public bool GetHasWarningValue() => hasWarning;

      /// <summary>
      /// Clears visible warnings.
      /// </summary>
      public void ResetWarning() => hasWarning = false;

      /// <summary>
      /// Redraws this class.
      /// </summary>
      public void RedrawItem()
      {
         this.Redraw();
      }

      /// <summary>
      /// Gets the glyph type value for display
      /// </summary>
      /// <returns>The type of glyph that should be displayed</returns>
      protected string GetGlyphTypeValue()
      {
         if (ModelRoot.ShowWarningsInDesigner && GetHasWarningValue())
            return "WarningGlyph";

         // ReSharper disable once ConvertIfStatementToReturnStatement
         if (IsAbstract)
            return "AbstractEntityGlyph";

         return "EntityGlyph";
      }

      #endregion

      /// <summary>
      /// Concurrency type, taking into account the model's default concurrency and any override defined in this class
      /// </summary>
      public ConcurrencyOverride EffectiveConcurrency
      {
         get
         {
            if (Concurrency != ConcurrencyOverride.Default)
               return Concurrency;

            return ModelRoot?.ConcurrencyDefault == EFModel.Concurrency.None ? ConcurrencyOverride.None : ConcurrencyOverride.Optimistic;
         }
      }

      /// <summary>
      ///    Calls the pre-reset method on the associated property value handler for each
      ///    tracking property of this model element.
      /// </summary>
      public virtual void PreResetIsTrackingProperties()
      {
         IsDatabaseSchemaTrackingPropertyHandler.Instance.PreResetValue(this);
         IsNamespaceTrackingPropertyHandler.Instance.PreResetValue(this);
         IsOutputDirectoryTrackingPropertyHandler.Instance.PreResetValue(this);

         // same with other tracking properties as they get added
      }

      /// <summary>
      ///    Calls the reset method on the associated property value handler for each
      ///    tracking property of this model element.
      /// </summary>
      internal virtual void ResetIsTrackingProperties()
      {
         IsDatabaseSchemaTrackingPropertyHandler.Instance.ResetValue(this);
         IsNamespaceTrackingPropertyHandler.Instance.ResetValue(this);
         IsOutputDirectoryTrackingPropertyHandler.Instance.ResetValue(this);
         // same with other tracking properties as they get added
      }

      /// <summary>
      /// All navigation properties including those in superclasses
      /// </summary>
      /// <param name="ignore">Associations to remove from the result</param>
      /// <returns>All navigation properties including those in superclasses, except those listed in the parameter</returns>
      public IEnumerable<NavigationProperty> AllNavigationProperties(params Association[] ignore)
      {
         List<NavigationProperty> result = LocalNavigationProperties(ignore).ToList();

         if (Superclass != null)
            result.AddRange(Superclass.AllNavigationProperties(ignore));

         return result;
      }

      /// <summary>
      /// All navigation properties defined in this class
      /// </summary>
      /// <param name="ignore">Associations to remove from the result</param>
      /// <returns>All navigation properties defined in this class, except those listed in the parameter</returns>
      public IEnumerable<NavigationProperty> LocalNavigationProperties(params Association[] ignore)
      {
         List<NavigationProperty> sourceProperties = Association.GetLinksToTargets(this)
                                                                .Except(ignore)
                                                                .Select(x => new NavigationProperty
                                                                             {
                                                                                Cardinality = x.TargetMultiplicity
                                                                              , ClassType = x.Target
                                                                              , AssociationObject = x
                                                                              , PropertyName = x.TargetPropertyName
                                                                              , Summary = x.TargetSummary
                                                                              , Description = x.TargetDescription
                                                                              , CustomAttributes = x.TargetCustomAttributes
                                                                              , DisplayText = x.TargetDisplayText
                                                                              , IsAutoProperty = true
                                                                              , ImplementNotify = x.TargetImplementNotify
                                                                              , FKPropertyName = x.TargetRole == EndpointRole.Principal ? x.FKPropertyName : null
                                                                             })
                                                                .ToList();

         List<NavigationProperty> targetProperties = Association.GetLinksToSources(this)
                                                                .Except(ignore)
                                                                .OfType<BidirectionalAssociation>()
                                                                .Select(x => new NavigationProperty
                                                                             {
                                                                                Cardinality = x.SourceMultiplicity
                                                                              , ClassType = x.Source
                                                                              , AssociationObject = x
                                                                              , PropertyName = x.SourcePropertyName
                                                                              , Summary = x.SourceSummary
                                                                              , Description = x.SourceDescription
                                                                              , CustomAttributes = x.SourceCustomAttributes
                                                                              , DisplayText = x.SourceDisplayText
                                                                              , IsAutoProperty = true
                                                                              , ImplementNotify = x.SourceImplementNotify
                                                                              , FKPropertyName = x.SourceRole == EndpointRole.Principal ? x.FKPropertyName : null
                                                                             })
                                                                .ToList();

         targetProperties.AddRange(Association.GetLinksToSources(this)
                                              .Except(ignore)
                                              .OfType<UnidirectionalAssociation>()
                                              .Select(x => new NavigationProperty
                                                           {
                                                              Cardinality = x.SourceMultiplicity
                                                            , ClassType = x.Source
                                                            , AssociationObject = x
                                                            , PropertyName = null
                                                            , FKPropertyName = x.SourceRole == EndpointRole.Principal ? x.FKPropertyName : null
                                                           }));
         int suffix = 0;
         foreach (NavigationProperty navigationProperty in targetProperties.Where(x => x.PropertyName == null))
         {
            navigationProperty.PropertyName = $"_{navigationProperty.ClassType.Name.ToLower()}{suffix++}";
            navigationProperty.ConstructorParameterOnly = true;
         }

         return sourceProperties.Concat(targetProperties);
      }

      /// <summary>
      /// required navigation (1.. cardinality) properties in this class
      /// </summary>
      /// <param name="ignore">Associations to remove from the result.</param>
      /// <returns>All required associations found, except for those in the [ignore] parameter</returns>
      public IEnumerable<NavigationProperty> RequiredNavigationProperties(params Association[] ignore) => LocalNavigationProperties(ignore).Where(x => x.Required).ToList();

      /// <summary>
      /// All the required navigation (1.. cardinality) properties in both this and base classes.
      /// </summary>
      /// <param name="ignore">Associations to remove from the result.</param>
      /// <returns>All required associations found, except for those in the [ignore] parameter</returns>
      public IEnumerable<NavigationProperty> AllRequiredNavigationProperties(params Association[] ignore) => AllNavigationProperties(ignore).Where(x => x.Required).ToList();

      /// <summary>
      /// Finds the association named by the value specified in the parameter
      /// </summary>
      /// <param name="identifier">Association property name to find.</param>
      /// <returns>The object representing the association, if could</returns>
      public NavigationProperty FindAssociationNamed(string identifier) => AllNavigationProperties().FirstOrDefault(x => x.PropertyName == identifier);

      /// <summary>
      /// Finds the attribute named by the value specified in the parameter 
      /// </summary>
      /// <param name="identifier">Attribute name to find.</param>
      /// <returns>The object representing the attribute, if could</returns>
      public ModelAttribute FindAttributeNamed(string identifier) => AllAttributes.FirstOrDefault(x => x.Name == identifier);

      /// <summary>
      /// Determines whether the generated code will have an association property with the name specified in the parameter
      /// </summary>
      /// <param name="identifier">Property name to find.</param>
      /// <returns>
      ///   <c>true</c> if the class will have this property; otherwise, <c>false</c>.
      /// </returns>
      public bool HasAssociationNamed(string identifier) => FindAssociationNamed(identifier) != null;

      /// <summary>
      /// Determines whether [has attribute named] [the specified identifier].
      /// </summary>
      /// <param name="identifier">The identifier.</param>
      /// <returns>
      ///   <c>true</c> if [has attribute named] [the specified identifier]; otherwise, <c>false</c>.
      /// </returns>
      public bool HasAttributeNamed(string identifier) => FindAttributeNamed(identifier) != null;

      /// <summary>
      /// Determines whether the generated code will have a property with the name specified in the parameter
      /// </summary>
      /// <param name="identifier">Property name to find.</param>
      /// <returns>
      ///   <c>true</c> if the class will have this property; otherwise, <c>false</c>.
      /// </returns>
      public bool HasPropertyNamed(string identifier) => HasAssociationNamed(identifier) || HasAttributeNamed(identifier);

      /// <summary>
      /// Gets the name of the superclass, if any.
      /// </summary>
      /// <returns></returns>
      private string GetBaseClassValue() => Superclass?.Name;

      /// <summary>
      /// Sets the superclass to the class with the supplied name, if it exists. Sets to null if can't be found.
      /// </summary>
      /// <param name="newValue">Simple name (not FQN) of class to use as superclass.</param>
      private void SetBaseClassValue(string newValue)
      {
         ModelClass baseClass = Store.ElementDirectory.FindElements<ModelClass>().FirstOrDefault(x => x.Name == newValue);
         Superclass?.Subclasses?.Remove(this);
         baseClass?.Subclasses?.Add(this);
         //Superclass = null;
         //Superclass = baseClass;
      }

      #region Validations

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      private void ClassShouldHaveAttributes(ValidationContext context)
      {
         if (ModelRoot == null) return;

         if (!Attributes.Any() && !LocalNavigationProperties().Any())
         {
            context.LogWarning($"{Name}: Class has no properties", "MCWNoProperties", this);
            hasWarning = true;
            RedrawItem();
         }
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]

      private void AttributesCannotBeNamedSameAsEnclosingClass(ValidationContext context)
      {
         if (ModelRoot == null) return;

         if (HasPropertyNamed(Name))
            context.LogError($"{Name}: Properties can't be named the same as the enclosing class", "MCESameName", this);
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      private void PersistentClassesMustHaveIdentity(ValidationContext context)
      {
         if (ModelRoot == null) return;

         if (!IsDependentType && !AllIdentityAttributes.Any())
            context.LogError($"{Name}: Class has no identity property in inheritance chain", "MCENoIdentity", this);
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      private void DerivedClassesShouldNotHaveIdentity(ValidationContext context)
      {
         if (ModelRoot == null) return;

         if (Attributes.Any(x => x.IsIdentity))
         {
            ModelClass modelClass = Superclass;
            while (modelClass != null)
            {
               if (modelClass.Attributes.Any(x => x.IsIdentity))
               {
                  context.LogWarning($"{modelClass.Name}: Identity attribute in derived class {Name} becomes a composite key", "MCWDerivedIdentity", this);
                  hasWarning = true;
                  RedrawItem();
                  return;
               }

               modelClass = modelClass.Superclass;
            }
         }
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      private void SummaryDescriptionIsEmpty(ValidationContext context)
      {
         if (ModelRoot == null) return;

         ModelRoot modelRoot = Store.ElementDirectory.FindElements<ModelRoot>().FirstOrDefault();
         if (modelRoot?.WarnOnMissingDocumentation == true && string.IsNullOrWhiteSpace(Summary))
         {
            context.LogWarning($"Class {Name} should be documented", "AWMissingSummary", this);
            hasWarning = true;
            RedrawItem();
         }
      }

      #endregion Validations

      #region DatabaseSchema tracking property

      private string databaseSchemaStorage;

      private string GetDatabaseSchemaValue()
      {
         if (!this.IsLoading() && IsDatabaseSchemaTracking)
         {
            try
            {
               return Store.ModelRoot()?.DatabaseSchema;
            }
            catch (NullReferenceException)
            {
               return null;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return null;
            }
         }

         return databaseSchemaStorage;
      }

      private void SetDatabaseSchemaValue(string value)
      {
         databaseSchemaStorage = value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsDatabaseSchemaTracking = false;
      }

      internal sealed partial class IsDatabaseSchemaTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsDatabaseSchemaTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);
            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(DatabaseSchemaDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>Performs the reset operation for the IsDatabaseSchemaTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            object calculatedValue = null;
            ModelRoot modelRoot = element.Store.ModelRoot();

            try
            {
               calculatedValue = modelRoot?.DatabaseSchema;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if (calculatedValue != null && element.DatabaseSchema == (string)calculatedValue)
               element.isDatabaseSchemaTrackingPropertyStorage = true;
         }

         /// <summary>
         ///    Method to set IsDatabaseSchemaTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property
         ///    value.
         /// </param>
         internal void PreResetValue(ModelClass element) =>
            // Force the IsDatabaseSchemaTracking property to false so that the value  
            // of the DatabaseSchema property is retrieved from storage.  
            element.isDatabaseSchemaTrackingPropertyStorage = false;
      }

      #endregion DatabaseSchema tracking property

      #region Namespace tracking property

      private string namespaceStorage;

      private string GetNamespaceValue()
      {
         if (!this.IsLoading() && IsNamespaceTracking)
         {
            try
            {
               return DefaultNamespace;
            }
            catch (NullReferenceException)
            {
               return null;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return null;
            }
         }

         return namespaceStorage;
      }

      private void SetNamespaceValue(string value)
      {
         namespaceStorage = string.IsNullOrWhiteSpace(value) || value == DefaultNamespace ? null : value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsNamespaceTracking = namespaceStorage == null;
      }

      internal sealed partial class IsNamespaceTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsNamespaceTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);
            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(NamespaceDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>Performs the reset operation for the IsNamespaceTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            element.isNamespaceTrackingPropertyStorage = string.IsNullOrWhiteSpace(element.namespaceStorage);
         }

         /// <summary>
         ///    Method to set IsNamespaceTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property
         ///    value.
         /// </param>
         internal void PreResetValue(ModelClass element) =>
            // Force the IsNamespaceTracking property to false so that the value  
            // of the Namespace property is retrieved from storage.  
            element.isNamespaceTrackingPropertyStorage = false;
      }

      #endregion Namespace tracking property

      #region OutputDirectory tracking property

      private string outputDirectoryStorage;

      private string GetOutputDirectoryValue()
      {
         if (!this.IsLoading() && IsOutputDirectoryTracking)
         {
            try
            {
               return DefaultOutputDirectory;
            }
            catch (NullReferenceException)
            {
               return default;
            }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;

               return default;
            }
         }

         return outputDirectoryStorage;
      }

      private void SetOutputDirectoryValue(string value)
      {
         outputDirectoryStorage = string.IsNullOrWhiteSpace(value) || value == DefaultOutputDirectory ? null : value;

         if (!Store.InUndoRedoOrRollback && !this.IsLoading())
            IsOutputDirectoryTracking = (outputDirectoryStorage == null);
      }

      internal sealed partial class IsOutputDirectoryTrackingPropertyHandler
      {
         /// <summary>
         ///    Called after the IsOutputDirectoryTracking property changes.
         /// </summary>
         /// <param name="element">The model element that has the property that changed. </param>
         /// <param name="oldValue">The previous value of the property. </param>
         /// <param name="newValue">The new value of the property. </param>
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);
            if (!element.Store.InUndoRedoOrRollback && newValue)
            {
               DomainPropertyInfo propInfo = element.Store.DomainDataDirectory.GetDomainProperty(OutputDirectoryDomainPropertyId);
               propInfo.NotifyValueChange(element);
            }
         }

         /// <summary>Performs the reset operation for the IsOutputDirectoryTracking property for a model element.</summary>
         /// <param name="element">The model element that has the property to reset.</param>
         internal void ResetValue(ModelClass element)
         {
            object calculatedValue = null;
            ModelRoot modelRoot = element.Store.ModelRoot();

            try
            {
               calculatedValue = element.IsDependentType ? modelRoot?.StructOutputDirectory : element.ModelRoot?.EntityOutputDirectory;
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
               if (CriticalException.IsCriticalException(e))
                  throw;
            }

            if (calculatedValue != null && element.OutputDirectory == (string)calculatedValue)
               element.isOutputDirectoryTrackingPropertyStorage = true;
         }

         /// <summary>
         ///    Method to set IsOutputDirectoryTracking to false so that this instance of this tracking property is not
         ///    storage-based.
         /// </summary>
         /// <param name="element">
         ///    The element on which to reset the property value.
         /// </param>
         internal void PreResetValue(ModelClass element) =>
            // Force the IsOutputDirectoryTracking property to false so that the value  
            // of the OutputDirectory property is retrieved from storage.  
            element.isOutputDirectoryTrackingPropertyStorage = false;
      }

      #endregion OutputDirectory tracking property

      #region IsImplementNotify tracking property

      /// <summary>
      /// Updates tracking properties when the IsImplementNotify value changes
      /// </summary>
      /// <param name="oldValue">Prior value</param>
      /// <param name="newValue">Current value</param>
      protected virtual void OnIsImplementNotifyChanged(bool oldValue, bool newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store, 
                                                         Attributes, 
                                                         ModelAttribute.ImplementNotifyDomainPropertyId, 
                                                         ModelAttribute.IsImplementNotifyTrackingDomainPropertyId);
         TrackingHelper.UpdateTrackingCollectionProperty(Store, 
                                                         Store.ElementDirectory.AllElements.OfType<Association>().Where(a => a.Source?.FullName == FullName),
                                                         Association.TargetImplementNotifyDomainPropertyId, 
                                                         Association.IsTargetImplementNotifyTrackingDomainPropertyId);
         TrackingHelper.UpdateTrackingCollectionProperty(Store, 
                                                         Store.ElementDirectory.AllElements.OfType<BidirectionalAssociation>().Where(a => a.Target?.FullName == FullName),
                                                         BidirectionalAssociation.SourceImplementNotifyDomainPropertyId, 
                                                         BidirectionalAssociation.IsSourceImplementNotifyTrackingDomainPropertyId);
      }

      internal sealed partial class ImplementNotifyPropertyHandler
      {
         protected override void OnValueChanged(ModelClass element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnIsImplementNotifyChanged(oldValue, newValue);
         }
      }

      #endregion IsImplementNotify tracking property

      internal void EnsureForeignKeyAttribute(string fkPropertyName, string type, bool required)
      {
         ModelAttribute fkProperty = Attributes.FirstOrDefault(a => a.Name == fkPropertyName);

         if (fkProperty == null)
         {
            fkProperty = new ModelAttribute(Store, new PropertyAssignment(ModelAttribute.NameDomainPropertyId, fkPropertyName));
            Attributes.Add(fkProperty);
         }

         fkProperty.Type = type;
         fkProperty.Indexed = true;
         fkProperty.Required = required;
      }
   }
}
