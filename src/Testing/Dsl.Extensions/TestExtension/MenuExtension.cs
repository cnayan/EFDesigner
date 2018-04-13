using System.Collections;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Diagrams.ExtensionEnablement;
using Microsoft.VisualStudio.Modeling.ExtensionEnablement;
using Sawczyn.EFDesigner.EFModel.ExtensionEnablement;

// adapted from https://docs.microsoft.com/en-us/visualstudio/modeling/extend-your-dsl-by-using-mef

namespace TestExtension
{
   [EFModelCommandExtension]
   public class TestMenuExtension : ICommandExtension
   {
      /// <summary>
      ///    Provides access to current document and selection.
      /// </summary>
      [Import]
      private IVsSelectionContext SelectionContext { get; set; }

      /// <summary>
      ///    Called when the user selects this command.
      /// </summary>
      /// <param name="command"></param>
      public void Execute(IMenuCommand command)
      {
         // Transaction is required if you want to update elements.  
         //using (Transaction t = SelectionContext.CurrentStore  
         //                                       .TransactionManager.BeginTransaction("fix names"))  
         //{
         foreach (ModelElement element in SelectionContext.CurrentSelection.Cast<ShapeElement>().Select(shape => shape.ModelElement))
         {
            MessageBox.Show(element.GetDomainClass().Name);
         }

         //t.Commit();  
         //}
      }

      /// <summary>
      ///    Called when the user right-clicks the diagram.
      ///    Determines whether the command should appear.
      ///    This method should set command.Enabled and command.Visible.
      /// </summary>
      /// <param name="command"></param>
      public void QueryStatus(IMenuCommand command)
      {
         ICollection currentSelection = SelectionContext?.CurrentSelection;
         command.Enabled = command.Visible = currentSelection != null && currentSelection.OfType<ShapeElement>().Any();
      }

      /// <summary>
      ///    Called when the user right-clicks the diagram.
      ///    Determines the text of the command in the menu.
      /// </summary>
      public string Text => "Show selected types";
   }
}
