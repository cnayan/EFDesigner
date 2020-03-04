﻿
















//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using DslModeling = global::Microsoft.VisualStudio.Modeling;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
using DslDiagrams = global::Microsoft.VisualStudio.Modeling.Diagrams;


[module: global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "type", Target = "Sawczyn.EFDesigner.EFModel.EFModelDiagram")]


namespace Sawczyn.EFDesigner.EFModel
{


	/// <summary>
	/// Double derived implementation for the rule that initiates view fixup when an element that has an associated shape is added to the model.
	/// This now enables the DSL author to everride the SkipFixUp() method 
	/// </summary>
	internal partial class FixUpAllDiagramsBase : DslModeling::AddRule
	{
		protected virtual bool SkipFixup(DslModeling::ModelElement childElement)
		{
			return childElement.IsDeleted;
		}
	}

	/// <summary>
	/// Rule that initiates view fixup when an element that has an associated shape is added to the model. 
	/// </summary>

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.ModelClass), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddShapeParentExistRulePriority, InitiallyDisabled=true)]

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.ModelEnum), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddShapeParentExistRulePriority, InitiallyDisabled=true)]

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.BidirectionalAssociation), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddConnectionRulePriority, InitiallyDisabled=true)]

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.UnidirectionalAssociation), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddConnectionRulePriority, InitiallyDisabled=true)]

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.Comment), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddShapeParentExistRulePriority, InitiallyDisabled=true)]

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.Generalization), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddConnectionRulePriority, InitiallyDisabled=true)]

	[DslModeling::RuleOn(typeof(global::Sawczyn.EFDesigner.EFModel.CommentReferencesSubjects), FireTime = DslModeling::TimeToFire.TopLevelCommit, Priority = DslDiagrams::DiagramFixupConstants.AddConnectionRulePriority, InitiallyDisabled=true)]

	internal sealed partial class FixUpAllDiagrams : FixUpAllDiagramsBase
	{
		public static void FixUp(DslDiagrams::Diagram diagram, DslModeling::ModelElement existingParent, DslModeling::ModelElement newChild)
        {
            if (existingParent == null)
            {
                throw new global::System.ArgumentNullException("existingParent");
            }
            if (newChild == null)
            {
                throw new global::System.ArgumentNullException("newChild");
            }
            if (!existingParent.IsDeleted && !newChild.IsDeleted)
            {
                foreach (var subject in DslModeling::DomainRoleInfo.GetElementLinks<DslDiagrams::PresentationViewsSubject>(existingParent, DslDiagrams::PresentationViewsSubject.SubjectDomainRoleId))
                {
                    var l_presentation = subject.Presentation as DslDiagrams::ShapeElement;
                    if (l_presentation != null && l_presentation.Diagram == diagram)
                    {
                        var newChildShape = l_presentation.FixUpChildShapes(newChild);
                        if (newChildShape != null)
                        {
                            var l_diagram = newChildShape.Diagram;
                            if (l_diagram != null && l_diagram == diagram)
                            {
                                l_diagram.FixUpDiagramSelection(newChildShape);
                            }
                        }
                    }
                }
            }
        }
	
		public static void FixUp(DslDiagrams::Diagram diagram, DslModeling::ModelElement element)
		{
			DslModeling::ModelElement parentElement;

			if(element is DslModeling::ElementLink)
			{
				parentElement = GetParentForRelationship(diagram, (DslModeling::ElementLink)element);
			} else

			if(element is global::Sawczyn.EFDesigner.EFModel.ModelClass)
			{

				parentElement = GetParentForModelClass((global::Sawczyn.EFDesigner.EFModel.ModelClass)element);
			} else

			if(element is global::Sawczyn.EFDesigner.EFModel.ModelEnum)
			{

				parentElement = GetParentForModelEnum((global::Sawczyn.EFDesigner.EFModel.ModelEnum)element);
			} else

			if(element is global::Sawczyn.EFDesigner.EFModel.Comment)
			{

				parentElement = GetParentForComment((global::Sawczyn.EFDesigner.EFModel.Comment)element);
			} else

			{
				parentElement = null;
			}
			
			if(parentElement != null)
			{
				FixUp(diagram, parentElement, element);
			}
		}
	
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public override void ElementAdded(DslModeling::ElementAddedEventArgs e)
		{
			if(e == null) throw new global::System.ArgumentNullException("e");
		
			var childElement = e.ModelElement;
			if (this.SkipFixup(childElement))
				return;
				
			var l_diagrams = e.ModelElement.Store.ElementDirectory.FindElements<DslDiagrams::Diagram>();
            foreach(var diagram in l_diagrams)
            {
                FixUp(diagram, childElement);
            }
		}
		public static global::Sawczyn.EFDesigner.EFModel.ModelRoot GetParentForModelClass( global::Sawczyn.EFDesigner.EFModel.ModelClass root )
			{
				// Segments 0 and 1
				global::Sawczyn.EFDesigner.EFModel.ModelRoot result = root.ModelRoot;
				if ( result == null ) return null;
				return result;
			}
			public static global::Sawczyn.EFDesigner.EFModel.ModelRoot GetParentForComment( global::Sawczyn.EFDesigner.EFModel.Comment root )
			{
				// Segments 0 and 1
				global::Sawczyn.EFDesigner.EFModel.ModelRoot result = root.ModelRoot;
				if ( result == null ) return null;
				return result;
			}
			public static global::Sawczyn.EFDesigner.EFModel.ModelRoot GetParentForModelEnum( global::Sawczyn.EFDesigner.EFModel.ModelEnum root )
			{
				// Segments 0 and 1
				global::Sawczyn.EFDesigner.EFModel.ModelRoot result = root.ModelRoot;
				if ( result == null ) return null;
				return result;
			}
	
		private static DslModeling::ModelElement GetParentForRelationship(DslDiagrams::Diagram diagram, DslModeling::ElementLink elementLink)
        {
            global::System.Collections.ObjectModel.ReadOnlyCollection<DslModeling::ModelElement> linkedElements = elementLink.LinkedElements;

            if (linkedElements.Count == 2)
            {
                DslDiagrams::ShapeElement sourceShape = linkedElements[0] as DslDiagrams::ShapeElement;
                DslDiagrams::ShapeElement targetShape = linkedElements[1] as DslDiagrams::ShapeElement;

                if (sourceShape == null)
                {
                    DslModeling::LinkedElementCollection<DslDiagrams::PresentationElement> presentationElements = DslDiagrams::PresentationViewsSubject.GetPresentation(linkedElements[0]);
                    foreach (DslDiagrams::PresentationElement presentationElement in presentationElements)
                    {
                        DslDiagrams::ShapeElement shape = presentationElement as DslDiagrams::ShapeElement;
                        if (shape != null && shape.Diagram == diagram)
                        {
                            sourceShape = shape;
                            break;
                        }
                    }
                }

                if (targetShape == null)
                {
                    DslModeling::LinkedElementCollection<DslDiagrams::PresentationElement> presentationElements = DslDiagrams::PresentationViewsSubject.GetPresentation(linkedElements[1]);
                    foreach (DslDiagrams::PresentationElement presentationElement in presentationElements)
                    {
                        DslDiagrams::ShapeElement shape = presentationElement as DslDiagrams::ShapeElement;
                        if (shape != null && shape.Diagram == diagram)
                        {
                            targetShape = shape;
                            break;
                        }
                    }
                }

                if (sourceShape == null || targetShape == null)
                {
                    global::System.Diagnostics.Debug.Write("Unable to find source and/or target shape for view fixup.");
                    return null;
                }

                DslDiagrams::ShapeElement sourceParent = sourceShape as DslDiagrams::Diagram ?? sourceShape.ParentShape;
                DslDiagrams::ShapeElement targetParent = targetShape.ParentShape;

                while (sourceParent != targetParent && sourceParent != null)
                {
                    DslDiagrams::ShapeElement curParent = targetParent;
                    while (sourceParent != curParent && curParent != null)
                    {
                        curParent = curParent.ParentShape;
                    }

                    if (sourceParent == curParent)
                    {
                        break;
                    }
                    else
                    {
                        sourceParent = sourceParent.ParentShape;
                    }
                }

                while (sourceParent != null)
                {
                    // ensure that the parent can parent connectors (i.e., a diagram or a swimlane).
                    if (sourceParent is DslDiagrams::Diagram || sourceParent is DslDiagrams::SwimlaneShape)
                    {
                        break;
                    }
                    else
                    {
                        sourceParent = sourceParent.ParentShape;
                    }
                }

                global::System.Diagnostics.Debug.Assert(sourceParent != null && sourceParent.ModelElement != null, "Unable to find common parent for view fixup.");
                return sourceParent.ModelElement;
            }

            return null;
        }
	}
}
