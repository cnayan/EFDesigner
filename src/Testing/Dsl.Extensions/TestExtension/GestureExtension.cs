using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Diagrams.ExtensionEnablement;
using Sawczyn.EFDesigner.EFModel;
using Sawczyn.EFDesigner.EFModel.ExtensionEnablement;

// adapted from https://docs.microsoft.com/en-us/visualstudio/modeling/extend-your-dsl-by-using-mef

namespace TestExtension
{
   [EFModelGestureExtension]
   public class GestureExtension : IGestureExtension
   {
      public void OnDoubleClick(ShapeElement targetElement, DiagramPointEventArgs diagramPointEventArgs)
      {
         MessageBox.Show("double click!");
      }

      /// <summary>
      ///    Called when the user drags anything over the diagram.
      ///    Return true if the dragged object can be dropped on the current target.
      /// </summary>
      /// <param name="targetMergeElement">The shape or diagram that the mouse is currently over</param>
      /// <param name="diagramDragEventArgs">Data about the dragged element.</param>
      /// <returns></returns>
      public bool CanDragDrop(ShapeElement targetMergeElement, DiagramDragEventArgs diagramDragEventArgs)
      {
         // This handler only allows items to be dropped onto the diagram:  
         return targetMergeElement is EFModelDiagram &&

                // And only accepts files dragged from Windows Explorer:  
                diagramDragEventArgs.Data.GetFormats().Contains("FileNameW");
      }

      /// <summary>
      ///    Called when the user drops an item onto the diagram.
      /// </summary>
      /// <param name="targetDropElement"></param>
      /// <param name="diagramDragEventArgs"></param>
      public void OnDragDrop(ShapeElement targetDropElement, DiagramDragEventArgs diagramDragEventArgs)
      {
         if (!(targetDropElement is EFModelDiagram)) 
            return;

         // This handler only accepts files dragged from Windows Explorer:  
         if (!(diagramDragEventArgs.Data.GetData("FileNameW") is string[] draggedFileNames) || draggedFileNames.Length == 0) 
            return;

         MessageBox.Show($"You dropped {string.Join(", ", draggedFileNames)}");

         //using (Transaction t = diagram.Store.TransactionManager.BeginTransaction("file names"))
         //{
         //   // Create an element to represent each file:  
         //   foreach (string fileName in draggedFileNames)
         //   {
         //      ModelClass element = new ModelClass(diagram.ModelElement.Partition);
         //      element.Name = fileName;

         //      // This method of adding the new element allows the position  
         //      // of the shape to be specified:            
         //      ElementGroup group = new ElementGroup(element);
         //      diagram.ElementOperations.MergeElementGroupPrototype(
         //        diagram, group.CreatePrototype(), PointD.ToPointF(diagramDragEventArgs.MousePosition));
         //   }
         //   t.Commit();
         //}
      }
   }
}
