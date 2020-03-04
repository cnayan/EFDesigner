﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
// ReSharper disable RedundantNameQualifier

namespace Sawczyn.EFDesigner.EFModel.DslPackage.TextTemplates.EditingOnly
{
   [SuppressMessage("ReSharper", "UnusedMember.Local")]
   [SuppressMessage("ReSharper", "UnusedMember.Global")]
   partial class EditOnly
   {
      /**************************************************
       * Code generation methods and data common to EF6 and EFCore
       */

      string[] NonNullableTypes
      {
         get
         {
            return new[]
                     {
                 "Binary"
               , "Geography"
               , "GeographyCollection"
               , "GeographyLineString"
               , "GeographyMultiLineString"
               , "GeographyMultiPoint"
               , "GeographyMultiPolygon"
               , "GeographyPoint"
               , "GeographyPolygon"
               , "Geometry"
               , "GeometryCollection"
               , "GeometryLineString"
               , "GeometryMultiLineString"
               , "GeometryMultiPoint"
               , "GeometryMultiPolygon"
               , "GeometryPoint"
               , "GeometryPolygon"
               , "String"
               };
         }
      }

      string[] SpatialTypes
      {
         get
         {
            return new[]
                     {
                 "Geography"
               , "GeographyCollection"
               , "GeographyLineString"
               , "GeographyMultiLineString"
               , "GeographyMultiPoint"
               , "GeographyMultiPolygon"
               , "GeographyPoint"
               , "GeographyPolygon"
               , "Geometry"
               , "GeometryCollection"
               , "GeometryLineString"
               , "GeometryMultiLineString"
               , "GeometryMultiPoint"
               , "GeometryMultiPolygon"
               , "GeometryPoint"
               , "GeometryPolygon"
               };
         }
      }

      private static string CreateShadowPropertyName(Association association, List<string> foreignKeyColumns, ModelAttribute identityAttribute)
      {
         string shadowNameBase = association.SourceRole == EndpointRole.Dependent
                              ? association.TargetPropertyName
                              : association is BidirectionalAssociation b
                                 ? b.SourcePropertyName
                                 : $"{association.Source.Name}.{association.TargetPropertyName}";

         string shadowPropertyName = $"{shadowNameBase}_{identityAttribute.Name}";

         int index = 0;

         while (foreignKeyColumns.Contains(shadowPropertyName))
            shadowPropertyName = $"{shadowNameBase}{++index}_{identityAttribute.Name}";

         return shadowPropertyName;
      }

      bool AllSuperclassesAreNullOrAbstract(ModelClass modelClass)
      {
         ModelClass superClass = modelClass.Superclass;

         while (superClass != null)
         {
            if (!superClass.IsAbstract)
               return false;

            superClass = superClass.Superclass;
         }

         return true;
      }

      void BeginNamespace(string ns)
      {
         if (!string.IsNullOrEmpty(ns))
         {
            Output($"namespace {ns}");
            Output("{");
         }
      }

      void EndNamespace(string ns)
      {
         if (!string.IsNullOrEmpty(ns))
            Output("}");
      }

      string FullyQualified(ModelRoot modelRoot, string typeName)
      {
         string[] parts = typeName.Split('.');

         if (parts.Length == 1)
            return typeName;

         string simpleName = parts[0];
         ModelEnum modelEnum = modelRoot.Store.ElementDirectory.AllElements.OfType<ModelEnum>().FirstOrDefault(e => e.Name == simpleName);

         return modelEnum != null
                     ? $"{modelEnum.FullName}.{parts.Last()}"
                     : typeName;
      }

      void GeneratePropertyAnnotations(ModelAttribute modelAttribute)
      {
         if (modelAttribute.Persistent)
         {
            if (modelAttribute.IsIdentity)
               Output("[Key]");

            if (modelAttribute.IsConcurrencyToken)
               Output("[ConcurrencyCheck]");
         }
         else
            Output("[NotMapped]");

         if (modelAttribute.Required)
            Output("[Required]");

         if (modelAttribute.FQPrimitiveType == "string")
         {
            if (modelAttribute.MinLength > 0)
               Output($"[MinLength({modelAttribute.MinLength})]");

            if (modelAttribute.MaxLength > 0)
            {
               Output($"[MaxLength({modelAttribute.MaxLength})]");
               Output($"[StringLength({modelAttribute.MaxLength})]");
            }
         }

         if (!string.IsNullOrWhiteSpace(modelAttribute.DisplayText))
            Output($"[Display(Name=\"{modelAttribute.DisplayText}\")]");
      }

      string GetFullContainerName(string containerType, string payloadType)
      {
         string result;

         switch (containerType)
         {
            case "HashSet":
               result = "System.Collections.Generic.HashSet<T>";

               break;

            case "LinkedList":
               result = "System.Collections.Generic.LinkedList<T>";

               break;

            case "List":
               result = "System.Collections.Generic.List<T>";

               break;

            case "SortedSet":
               result = "System.Collections.Generic.SortedSet<T>";

               break;

            case "Collection":
               result = "System.Collections.ObjectModel.Collection<T>";

               break;

            case "ObservableCollection":
               result = "System.Collections.ObjectModel.ObservableCollection<T>";

               break;

            case "BindingList":
               result = "System.ComponentModel.BindingList<T>";

               break;

            default:
               result = containerType;

               break;
         }

         if (result.EndsWith("<T>"))
            result = result.Replace("<T>", $"<{payloadType}>");

         return result;
      }

      List<string> GetRequiredParameterNames(ModelClass modelClass, bool? publicOnly = null)
      {
         List<string> requiredParameterNames = modelClass.AllRequiredAttributes
                                                   .Where(x => (!x.IsIdentity || x.IdentityType == IdentityType.Manual)
                                                            && !x.IsConcurrencyToken
                                                            && (x.SetterVisibility == SetterAccessModifier.Public || publicOnly != false)
                                                            && string.IsNullOrEmpty(x.InitialValue))
                                                   .Select(x => x.Name.ToLower())
                                                   .ToList();

         requiredParameterNames.AddRange(modelClass.AllRequiredNavigationProperties()
                                                   .Where(np => np.AssociationObject.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One
                                                               || np.AssociationObject.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One)
                                                   .Select(x => x.PropertyName.ToLower()));

         requiredParameterNames.AddRange(modelClass.AllRequiredAttributes
                                                   .Where(x => (!x.IsIdentity || x.IdentityType == IdentityType.Manual)
                                                            && !x.IsConcurrencyToken
                                                            && (x.SetterVisibility == SetterAccessModifier.Public || publicOnly != false)
                                                            && !string.IsNullOrEmpty(x.InitialValue))
                                                   .Select(x => x.Name.ToLower()));

         return requiredParameterNames;
      }

      List<string> GetRequiredParameters(ModelClass modelClass, bool? haveDefaults, bool? publicOnly = null)
      {
         List<string> requiredParameters = new List<string>();

         if (haveDefaults != true)
         {
            requiredParameters.AddRange(modelClass.AllRequiredAttributes
                                                   .Where(x => (!x.IsIdentity || x.IdentityType == IdentityType.Manual)
                                                            && !x.IsConcurrencyToken
                                                            && (x.SetterVisibility == SetterAccessModifier.Public || publicOnly != false)
                                                            && string.IsNullOrEmpty(x.InitialValue))
                                                   .Select(x => $"{x.FQPrimitiveType} {x.Name.ToLower()}"));

            // don't use 1..1 associations in constructor parameters. Becomes a Catch-22 scenario.
            requiredParameters.AddRange(modelClass.AllRequiredNavigationProperties()
                                                   .Where(np => np.AssociationObject.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One
                                                            || np.AssociationObject.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One)
                                                   .Select(x => $"{x.ClassType.FullName} {x.PropertyName.ToLower()}"));
         }

         if (haveDefaults != false)
         {
            requiredParameters.AddRange(modelClass.AllRequiredAttributes
                                                   .Where(x => (!x.IsIdentity || x.IdentityType == IdentityType.Manual)
                                                            && !x.IsConcurrencyToken
                                                            && (x.SetterVisibility == SetterAccessModifier.Public || publicOnly != false)
                                                            && !string.IsNullOrEmpty(x.InitialValue))
                                                   .Select(x =>
                                                            {
                                                               string quote = x.PrimitiveType == "string"
                                                                           ? "\""
                                                                           : x.PrimitiveType == "char"
                                                                              ? "'"
                                                                              : string.Empty;

                                                               string value = FullyQualified(modelClass.ModelRoot, x.InitialValue.Trim('"', '\''));

                                                               return $"{x.FQPrimitiveType} {x.Name.ToLower()} = {quote}{value}{quote}";
                                                            }));
         }

         return requiredParameters;
      }

      bool IsNullable(ModelAttribute modelAttribute)
      {
         return !modelAttribute.Required && !modelAttribute.IsIdentity && !modelAttribute.IsConcurrencyToken && !NonNullableTypes.Contains(modelAttribute.Type);
      }

      void NL()
      {
         WriteLine(string.Empty);
      }

      void Output(string text)
      {
         if (text.StartsWith("}"))
            PopIndent();

         WriteLine(text);

         if (text.EndsWith("{"))
            PushIndent("   ");
      }

      void Output(string template, params object[] items)
      {
         string text = string.Format(template, items);
         Output(text);
      }

      void OutputChopped(IEnumerable<string> segments)
      {
         string[] segmentArray = segments?.ToArray() ?? new string[0];

         if (!segmentArray.Any())
            return;

         int indent = segmentArray[0].IndexOf('.');

         if (indent == -1)
         {
            if (segmentArray.Length > 1)
            {
               segmentArray[0] = $"{segmentArray[0]}.{segmentArray[1]}";
               indent = segmentArray[0].IndexOf('.');
               segmentArray = segmentArray.Where((source, index) => index != 1).ToArray();
            }
         }

         for (int index = 1; index < segmentArray.Length; ++index)
            segmentArray[index] = $"{new string(' ', indent)}.{segmentArray[index]}";

         if (!segmentArray[segmentArray.Length - 1].Trim().EndsWith(";"))
            segmentArray[segmentArray.Length - 1] = segmentArray[segmentArray.Length - 1] + ";";

         foreach (string segment in segmentArray)
            Output(segment);
      }

      void WriteClass(ModelClass modelClass)
      {
         Output("using System;");
         Output("using System.Collections.Generic;");
         Output("using System.Collections.ObjectModel;");
         Output("using System.ComponentModel;");
         Output("using System.ComponentModel.DataAnnotations;");
         Output("using System.ComponentModel.DataAnnotations.Schema;");
         Output("using System.Linq;");
         Output("using System.Runtime.CompilerServices;");
         List<string> additionalUsings = GetAdditionalUsingStatementsEF6(modelClass.ModelRoot);

         if (additionalUsings.Any())
            Output(string.Join("\n", additionalUsings));

         NL();

         BeginNamespace(modelClass.EffectiveNamespace);

         string isAbstract = modelClass.IsAbstract
                           ? "abstract "
                           : string.Empty;

         List<string> bases = new List<string>();

         if (modelClass.Superclass != null)
            bases.Add(modelClass.Superclass.FullName);

         if (!string.IsNullOrEmpty(modelClass.CustomInterfaces))
            bases.AddRange(modelClass.CustomInterfaces.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

         if (modelClass.ImplementNotify)
            bases.Add("INotifyPropertyChanged");

         string baseClass = string.Join(", ", bases.Select(x => x.Trim()));

         if (!string.IsNullOrEmpty(modelClass.Summary))
         {
            Output("/// <summary>");
            WriteCommentBody(modelClass.Summary);
            Output("/// </summary>");
         }

         if (!string.IsNullOrEmpty(modelClass.Description))
         {
            Output("/// <remarks>");
            WriteCommentBody(modelClass.Description);
            Output("/// </remarks>");
         }

         if (!string.IsNullOrWhiteSpace(modelClass.CustomAttributes))
            Output($"[{modelClass.CustomAttributes.Trim('[', ']')}]");

         Output(baseClass.Length > 0
                     ? $"public {isAbstract}partial class {modelClass.Name}: {baseClass}"
                     : $"public {isAbstract}partial class {modelClass.Name}");

         Output("{");

         WriteConstructor(modelClass);
         WriteProperties(modelClass);
         WriteNavigationProperties(modelClass);
         WriteNotifyPropertyChanged(modelClass);

         Output("}");

         EndNamespace(modelClass.EffectiveNamespace);
         NL();
      }

      void WriteCommentBody(string comment)
      {
         int chunkSize = 80;
         string[] parts = comment.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

         foreach (string value in parts)
         {
            string text = value;

            while (text.Length > 0)
            {
               string outputText = text;

               if (outputText.Length > chunkSize)
               {
                  outputText = (text.IndexOf(' ', chunkSize) > 0
                                    ? text.Substring(0, text.IndexOf(' ', chunkSize))
                                    : text).Trim();

                  text = text.Substring(outputText.Length).Trim();
               }
               else
                  text = string.Empty;

               Output("/// " + outputText.Replace("<", "{").Replace(">", "}"));
            }
         }
      }

      void WriteConstructor(ModelClass modelClass)
      {
         Output("partial void Init();");
         NL();

         /***********************************************************************/
         // Default constructor
         /***********************************************************************/

         bool hasRequiredParameters = GetRequiredParameters(modelClass, false).Any();

         bool hasOneToOneAssociations = modelClass.AllRequiredNavigationProperties()
                                             .Any(np => np.AssociationObject.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One
                                                      && np.AssociationObject.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One);

         string visibility = (hasRequiredParameters || modelClass.IsAbstract) && !modelClass.IsDependentType
                           ? "protected"
                           : "public";

         if (visibility == "public")
         {
            Output("/// <summary>");
            Output("/// Default constructor");
            Output("/// </summary>");
         }
         else if (modelClass.IsAbstract)
         {
            Output("/// <summary>");
            Output("/// Default constructor. Protected due to being abstract.");
            Output("/// </summary>");
         }
         else if (hasRequiredParameters)
         {
            Output("/// <summary>");
            Output("/// Default constructor. Protected due to required properties, but present because EF needs it.");
            Output("/// </summary>");
         }

         List<string> remarks = new List<string>();

         if (hasOneToOneAssociations)
         {
            List<Association> oneToOneAssociations = modelClass.AllRequiredNavigationProperties()
                                                         .Where(np => np.AssociationObject.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One
                                                                     && np.AssociationObject.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One)
                                                         .Select(np => np.AssociationObject)
                                                         .ToList();

            List<ModelClass> otherEndsOneToOne = oneToOneAssociations.Where(a => a.Source != modelClass).Select(a => a.Target)
                                                               .Union(oneToOneAssociations.Where(a => a.Target != modelClass).Select(a => a.Source))
                                                               .ToList();

            if (oneToOneAssociations.Any(a => a.Source.Name == modelClass.Name && a.Target.Name == modelClass.Name))
               otherEndsOneToOne.Add(modelClass);

            if (otherEndsOneToOne.Any())
            {
               string nameList = otherEndsOneToOne.Count == 1
                              ? otherEndsOneToOne.First().Name
                              : string.Join(", ", otherEndsOneToOne.Take(otherEndsOneToOne.Count - 1).Select(c => c.Name))
                              + " and "
                              + (otherEndsOneToOne.Last().Name != modelClass.Name
                                    ? otherEndsOneToOne.Last().Name
                                    : "itself");

               remarks.Add($"// NOTE: This class has one-to-one associations with {nameList}.");
               remarks.Add("// One-to-one associations are not validated in constructors since this causes a scenario where each one must be constructed before the other.");
            }
         }

         Output(modelClass.Superclass != null
                     ? $"{visibility} {modelClass.Name}(): base()"
                     : $"{visibility} {modelClass.Name}()");

         Output("{");

         if (remarks.Count > 0)
         {
            foreach (string remark in remarks)
               Output(remark);

            NL();
         }

         WriteDefaultConstructorBody(modelClass);

         Output("}");
         NL();

         /***********************************************************************/
         // Constructor with required parameters (if necessary)
         /***********************************************************************/

         if (hasRequiredParameters)
         {
            visibility = modelClass.IsAbstract
                              ? "protected"
                              : "public";

            Output("/// <summary>");
            Output("/// Public constructor with required data");
            Output("/// </summary>");

            WriteConstructorComments(modelClass);
            Output($"{visibility} {modelClass.Name}({string.Join(", ", GetRequiredParameters(modelClass, true))})");
            Output("{");

            if (remarks.Count > 0)
            {
               foreach (string remark in remarks)
                  Output(remark);

               NL();
            }

            foreach (ModelAttribute requiredAttribute in modelClass.AllRequiredAttributes
                                                                     .Where(x => (!x.IsIdentity || x.IdentityType == IdentityType.Manual)
                                                                              && !x.IsConcurrencyToken
                                                                              && x.SetterVisibility == SetterAccessModifier.Public))
            {
               if (requiredAttribute.Type == "String")
                  Output($"if (string.IsNullOrEmpty({requiredAttribute.Name.ToLower()})) throw new ArgumentNullException(nameof({requiredAttribute.Name.ToLower()}));");
               else if (requiredAttribute.Type.StartsWith("Geo"))
                  Output($"if ({requiredAttribute.Name.ToLower()} == null) throw new ArgumentNullException(nameof({requiredAttribute.Name.ToLower()}));");

               Output($"this.{requiredAttribute.Name} = {requiredAttribute.Name.ToLower()};");
               NL();
            }

            foreach (ModelAttribute modelAttribute in modelClass.Attributes.Where(x => x.SetterVisibility == SetterAccessModifier.Public
                                                                                    && !x.Required
                                                                                    && !string.IsNullOrEmpty(x.InitialValue)
                                                                                    && x.InitialValue != "null"))
            {
               string quote = modelAttribute.Type == "String"
                           ? "\""
                           : modelAttribute.Type == "Char"
                              ? "'"
                              : string.Empty;

               Output($"this.{modelAttribute.Name} = {quote}{FullyQualified(modelClass.ModelRoot, modelAttribute.InitialValue)}{quote};");
            }

            foreach (NavigationProperty requiredNavigationProperty in modelClass.AllRequiredNavigationProperties()
                                                                                 .Where(np => np.AssociationObject.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One
                                                                                          || np.AssociationObject.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One))
            {
               string parameterName = requiredNavigationProperty.PropertyName.ToLower();
               Output($"if ({parameterName} == null) throw new ArgumentNullException(nameof({parameterName}));");

               if (requiredNavigationProperty.IsCollection)
                  Output($"{requiredNavigationProperty.PropertyName}.Add({parameterName});");
               else if (requiredNavigationProperty.ConstructorParameterOnly)
               {
                  UnidirectionalAssociation association = requiredNavigationProperty.AssociationObject as UnidirectionalAssociation;

                  Output(association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany
                              ? $"{requiredNavigationProperty.PropertyName}.{association.TargetPropertyName}.Add(this);"
                              : $"{requiredNavigationProperty.PropertyName}.{association.TargetPropertyName} = this;");
               }
               else
                  Output($"this.{requiredNavigationProperty.PropertyName} = {parameterName};");

               NL();
            }

            foreach (NavigationProperty navigationProperty in modelClass.LocalNavigationProperties()
                                                                        .Where(x => x.AssociationObject.Persistent && (x.IsCollection || x.ClassType.IsDependentType) && !x.ConstructorParameterOnly))
            {
               if (!navigationProperty.IsCollection)
                  Output($"this.{navigationProperty.PropertyName} = new {navigationProperty.ClassType.FullName}();");
               else
               {
                  string collectionType = GetFullContainerName(navigationProperty.AssociationObject.CollectionClass, navigationProperty.ClassType.FullName);
                  Output($"this.{navigationProperty.PropertyName} = new {collectionType}();");
               }
            }

            NL();
            Output("Init();");
            Output("}");
            NL();

            if (!modelClass.IsAbstract)
            {
               Output("/// <summary>");
               Output("/// Static create function (for use in LINQ queries, etc.)");
               Output("/// </summary>");
               WriteConstructorComments(modelClass);

               string newToken = string.Empty;
               List<string> requiredParameters = GetRequiredParameters(modelClass, null);

               if (!AllSuperclassesAreNullOrAbstract(modelClass))
               {
                  List<string> superclassRequiredParameters = GetRequiredParameters(modelClass.Superclass, null);

                  if (!requiredParameters.Except(superclassRequiredParameters).Any())
                     newToken = "new ";
               }

               Output($"public static {newToken}{modelClass.Name} Create({string.Join(", ", GetRequiredParameters(modelClass, null))})");
               Output("{");
               Output($"return new {modelClass.Name}({string.Join(", ", GetRequiredParameterNames(modelClass))});");
               Output("}");
               NL();
            }
         }
      }

      void WriteConstructorComments(ModelClass modelClass)
      {
         foreach (ModelAttribute requiredAttribute in modelClass.AllRequiredAttributes.Where(x => (!x.IsIdentity || x.IdentityType == IdentityType.Manual)
                                                                                                && !x.IsConcurrencyToken
                                                                                                && x.SetterVisibility == SetterAccessModifier.Public))
            Output($@"/// <param name=""{requiredAttribute.Name.ToLower()}"">{requiredAttribute.Summary}</param>");

         // TODO: Add comment if available
         foreach (NavigationProperty requiredNavigationProperty in modelClass.AllRequiredNavigationProperties()
                                                                              .Where(np => np.AssociationObject.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One
                                                                                       || np.AssociationObject.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One))
            Output($@"/// <param name=""{requiredNavigationProperty.PropertyName.ToLower()}""></param>");
      }

      void WriteDefaultConstructorBody(ModelClass modelClass)
      {
         int lineCount = 0;

         foreach (ModelAttribute modelAttribute in modelClass.Attributes.Where(x => x.SetterVisibility == SetterAccessModifier.Public
                                                                                 && !string.IsNullOrEmpty(x.InitialValue)
                                                                                 && x.InitialValue != "null"))
         {
            string quote = modelAttribute.Type == "String"
                        ? "\""
                        : modelAttribute.Type == "Char"
                           ? "'"
                           : string.Empty;

            Output($"{modelAttribute.Name} = {quote}{FullyQualified(modelClass.ModelRoot, modelAttribute.InitialValue)}{quote};");
            ++lineCount;
         }

         foreach (NavigationProperty navigationProperty in modelClass.LocalNavigationProperties()
                                                                     .Where(x => x.AssociationObject.Persistent && (x.IsCollection || x.ClassType.IsDependentType) && !x.ConstructorParameterOnly))
         {
            if (navigationProperty.ClassType.IsDependentType)
               Output($"{navigationProperty.PropertyName} = new {navigationProperty.ClassType.FullName}();");
            else
            {
               string collectionType = GetFullContainerName(navigationProperty.AssociationObject.CollectionClass, navigationProperty.ClassType.FullName);
               Output($"{navigationProperty.PropertyName} = new {collectionType}();");
            }

            ++lineCount;
         }

         if (lineCount > 0)
            NL();

         Output("Init();");
      }

      void WriteEnum(ModelEnum modelEnum)
      {
         Output("using System;");
         NL();

         BeginNamespace(modelEnum.EffectiveNamespace);

         if (!string.IsNullOrEmpty(modelEnum.Summary))
         {
            Output("/// <summary>");
            WriteCommentBody(modelEnum.Summary);
            Output("/// </summary>");
         }

         if (!string.IsNullOrEmpty(modelEnum.Description))
         {
            Output("/// <remarks>");
            WriteCommentBody(modelEnum.Description);
            Output("/// </remarks>");
         }

         if (modelEnum.IsFlags)
            Output("[Flags]");

         if (!string.IsNullOrWhiteSpace(modelEnum.CustomAttributes))
            Output($"[{modelEnum.CustomAttributes.Trim('[', ']')}]");

         Output($"public enum {modelEnum.Name} : {modelEnum.ValueType}");
         Output("{");

         ModelEnumValue[] values = modelEnum.Values.ToArray();

         for (int index = 0; index < values.Length; ++index)
         {
            if (!string.IsNullOrEmpty(values[index].Summary))
            {
               Output("/// <summary>");
               WriteCommentBody(values[index].Summary);
               Output("/// </summary>");
            }

            if (!string.IsNullOrEmpty(values[index].Description))
            {
               Output("/// <remarks>");
               WriteCommentBody(values[index].Description);
               Output("/// </remarks>");
            }

            if (!string.IsNullOrWhiteSpace(values[index].CustomAttributes))
               Output($"[{values[index].CustomAttributes.Trim('[', ']')}]");

            if (!string.IsNullOrWhiteSpace(values[index].DisplayText))
               Output($"[System.ComponentModel.DataAnnotations.Display(Name=\"{values[index].DisplayText}\")]");

            Output(string.IsNullOrEmpty(values[index].Value)
                        ? $"{values[index].Name}{(index < values.Length - 1 ? "," : string.Empty)}"
                        : $"{values[index].Name} = {values[index].Value}{(index < values.Length - 1 ? "," : string.Empty)}");
         }

         Output("}");

         EndNamespace(modelEnum.EffectiveNamespace);
      }

      void WriteNavigationProperties(ModelClass modelClass)
      {
         if (!modelClass.LocalNavigationProperties().Any(x => x.AssociationObject.Persistent))
            return;

         Output("/*************************************************************************");
         Output(" * Navigation properties");
         Output(" *************************************************************************/");
         NL();

         foreach (NavigationProperty navigationProperty in modelClass.LocalNavigationProperties().Where(x => !x.ConstructorParameterOnly))
         {
            string type = navigationProperty.IsCollection
                        ? $"ICollection<{navigationProperty.ClassType.FullName}>"
                        : navigationProperty.ClassType.FullName;

            if (!navigationProperty.IsCollection && !navigationProperty.IsAutoProperty)
            {
               Output($"protected {type} _{navigationProperty.PropertyName};");
               Output($"partial void Set{navigationProperty.PropertyName}({type} oldValue, ref {type} newValue);");
               Output($"partial void Get{navigationProperty.PropertyName}(ref {type} result);");

               NL();
            }

            List<string> comments = new List<string>();

            if (navigationProperty.Required)
               comments.Add("Required");

            string comment = comments.Count > 0
                           ? string.Join(", ", comments)
                           : string.Empty;

            if (!string.IsNullOrEmpty(navigationProperty.Summary) || !string.IsNullOrEmpty(comment))
            {
               Output("/// <summary>");

               if (!string.IsNullOrEmpty(comment) && !string.IsNullOrEmpty(navigationProperty.Summary))
                  comment = comment + "<br/>";

               if (!string.IsNullOrEmpty(comment))
                  WriteCommentBody(comment);

               if (!string.IsNullOrEmpty(navigationProperty.Summary))
                  WriteCommentBody(navigationProperty.Summary);

               Output("/// </summary>");
            }

            if (!string.IsNullOrEmpty(navigationProperty.Description))
            {
               Output("/// <remarks>");
               WriteCommentBody(navigationProperty.Description);
               Output("/// </remarks>");
            }

            if (!string.IsNullOrWhiteSpace(navigationProperty.CustomAttributes))
               Output($"[{navigationProperty.CustomAttributes.Trim('[', ']')}]");

            if (!string.IsNullOrWhiteSpace(navigationProperty.DisplayText))
               Output($"[Display(Name=\"{navigationProperty.DisplayText}\")]");

            if (navigationProperty.IsCollection)
               Output($"public virtual {type} {navigationProperty.PropertyName} {{ get; protected set; }}");
            else if (navigationProperty.IsAutoProperty)
               Output($"public virtual {type} {navigationProperty.PropertyName} {{ get; set; }}");
            else
            {
               Output($"public virtual {type} {navigationProperty.PropertyName}");
               Output("{");
               Output("get");
               Output("{");
               Output($"{type} value = _{navigationProperty.PropertyName};");
               Output($"Get{navigationProperty.PropertyName}(ref value);");
               Output($"return (_{navigationProperty.PropertyName} = value);");
               Output("}");
               Output("set");
               Output("{");
               Output($"{type} oldValue = _{navigationProperty.PropertyName};");
               Output($"Set{navigationProperty.PropertyName}(oldValue, ref value);");
               Output("if (oldValue != value)");
               Output("{");
               Output($"_{navigationProperty.PropertyName} = value;");

               if (navigationProperty.ImplementNotify)
                  Output("OnPropertyChanged();");

               Output("}");
               Output("}");
               Output("}");
            }

            NL();
         }
      }

      void WriteNotifyPropertyChanged(ModelClass modelClass)
      {
         if (modelClass.ImplementNotify || modelClass.LocalNavigationProperties().Any(x => x.ImplementNotify) || modelClass.Attributes.Any(x => x.ImplementNotify))
         {
            string modifier = modelClass.Superclass != null && modelClass.Superclass.ImplementNotify
                           ? "override"
                           : "virtual";

            Output($"public {modifier} event PropertyChangedEventHandler PropertyChanged;");
            NL();
            Output($"protected {modifier} void OnPropertyChanged([CallerMemberName] string propertyName = null)");
            Output("{");
            Output("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
            Output("}");
            NL();
         }
      }

      void WriteProperties(ModelClass modelClass)
      {
         Output("/*************************************************************************");
         Output(" * Properties");
         Output(" *************************************************************************/");
         NL();

         List<string> segments = new List<string>();

         foreach (ModelAttribute modelAttribute in modelClass.Attributes)
         {
            segments.Clear();

            if (modelAttribute.IsIdentity)
               segments.Add("Identity");

            if (modelAttribute.Indexed)
               segments.Add("Indexed");

            if (modelAttribute.Required || modelAttribute.IsIdentity)
               segments.Add("Required");

            if (modelAttribute.MinLength > 0)
               segments.Add($"Min length = {modelAttribute.MinLength}");

            if (modelAttribute.MaxLength > 0)
               segments.Add($"Max length = {modelAttribute.MaxLength}");

            if (!string.IsNullOrEmpty(modelAttribute.InitialValue))
            {
               string quote = modelAttribute.PrimitiveType == "string"
                           ? "\""
                           : modelAttribute.PrimitiveType == "char"
                              ? "'"
                              : string.Empty;

               segments.Add($"Default value = {quote}{FullyQualified(modelClass.ModelRoot, modelAttribute.InitialValue.Trim('"'))}{quote}");
            }

            string nullable = IsNullable(modelAttribute)
                           ? "?"
                           : string.Empty;

            if (!modelAttribute.IsConcurrencyToken && !modelAttribute.AutoProperty)
            {
               string visibility = modelAttribute.Indexed ? "internal" : "protected";
               Output("/// <summary>");
               Output($"/// Backing field for {modelAttribute.Name}");
               Output("/// </summary>");
               Output($"{visibility} {modelAttribute.FQPrimitiveType}{nullable} _{modelAttribute.Name};");
               Output("/// <summary>");
               Output($"/// When provided in a partial class, allows value of {modelAttribute.Name} to be changed before setting.");
               Output("/// </summary>");
               Output($"partial void Set{modelAttribute.Name}({modelAttribute.FQPrimitiveType}{nullable} oldValue, ref {modelAttribute.FQPrimitiveType}{nullable} newValue);");
               Output("/// <summary>");
               Output($"/// When provided in a partial class, allows value of {modelAttribute.Name} to be changed before returning.");
               Output("/// </summary>");
               Output($"partial void Get{modelAttribute.Name}(ref {modelAttribute.FQPrimitiveType}{nullable} result);");

               NL();
            }

            if (!string.IsNullOrEmpty(modelAttribute.Summary) || segments.Any())
            {
               Output("/// <summary>");

               if (segments.Any())
                  WriteCommentBody($"{string.Join(", ", segments)}");

               if (!string.IsNullOrEmpty(modelAttribute.Summary))
                  WriteCommentBody(modelAttribute.Summary);

               Output("/// </summary>");
            }

            if (!string.IsNullOrEmpty(modelAttribute.Description))
            {
               Output("/// <remarks>");
               WriteCommentBody(modelAttribute.Description);
               Output("/// </remarks>");
            }

            string setterVisibility = (modelAttribute.IsIdentity && modelAttribute.IdentityType != IdentityType.Manual) || modelAttribute.SetterVisibility == SetterAccessModifier.Protected
                                       ? "protected "
                                       : modelAttribute.SetterVisibility == SetterAccessModifier.Internal
                                          ? "internal "
                                          : string.Empty;

            GeneratePropertyAnnotations(modelAttribute);

            if (!string.IsNullOrWhiteSpace(modelAttribute.CustomAttributes))
               Output($"[{modelAttribute.CustomAttributes.Trim('[', ']')}]");

            if (modelAttribute.IsConcurrencyToken || modelAttribute.AutoProperty)
               Output($"public {modelAttribute.FQPrimitiveType}{nullable} {modelAttribute.Name} {{ get; {setterVisibility}set; }}");
            else
            {
               Output($"public {modelAttribute.FQPrimitiveType}{nullable} {modelAttribute.Name}");
               Output("{");
               Output("get");
               Output("{");
               Output($"{modelAttribute.FQPrimitiveType}{nullable} value = _{modelAttribute.Name};");
               Output($"Get{modelAttribute.Name}(ref value);");
               Output($"return (_{modelAttribute.Name} = value);");
               Output("}");
               Output($"{setterVisibility}set");
               Output("{");
               Output($"{modelAttribute.FQPrimitiveType}{nullable} oldValue = _{modelAttribute.Name};");
               Output($"Set{modelAttribute.Name}(oldValue, ref value);");
               Output("if (oldValue != value)");
               Output("{");
               Output($"_{modelAttribute.Name} = value;");

               if (modelAttribute.ImplementNotify)
                  Output("OnPropertyChanged();");

               Output("}");
               Output("}");
               Output("}");
            }

            NL();
         }

         if (!modelClass.AllAttributes.Any(x => x.IsConcurrencyToken)
            && (modelClass.Concurrency == ConcurrencyOverride.Optimistic || modelClass.ModelRoot.ConcurrencyDefault == Concurrency.Optimistic))
         {
            Output("/// <summary>");
            Output("/// Concurrency token");
            Output("/// </summary>");
            Output("[Timestamp]");
            Output("public Byte[] Timestamp { get; set; }");
            NL();
         }
      }

      private void WriteDbContextComments(ModelRoot modelRoot)
      {
         if (!string.IsNullOrEmpty(modelRoot.Summary))
         {
            Output("/// <summary>");
            WriteCommentBody(modelRoot.Summary);
            Output("/// </summary>");

            if (!string.IsNullOrEmpty(modelRoot.Description))
            {
               Output("/// <remarks>");
               WriteCommentBody(modelRoot.Description);
               Output("/// </remarks>");
            }
         }
         else
            Output("/// <inheritdoc/>");
      }
   }
}