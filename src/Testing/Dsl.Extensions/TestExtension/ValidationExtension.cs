using Microsoft.VisualStudio.Modeling.Validation;
using Sawczyn.EFDesigner.EFModel;
using Sawczyn.EFDesigner.EFModel.ExtensionEnablement;

// adapted from https://docs.microsoft.com/en-us/visualstudio/modeling/extend-your-dsl-by-using-mef

namespace TestExtension
{
   public class ValidationExtension // no special interface to implement
   {
      // SAMPLE VALIDATION METHOD.  
      // All validation methods have the following attributes.  

      /// <summary>
      ///    When validation is executed, this method is invoked
      ///    for every element in the model that is an instance
      ///    of the second parameter type.
      /// </summary>
      /// <param name="context">For reporting errors</param>
      /// <param name="elementToValidate"></param>
      [EFModelValidationExtension]
      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Open | ValidationCategories.Menu)]
      private void ValidateClassNames(ValidationContext context, ModelClass elementToValidate)
      {
         // Write code here to test values and links.  
         if (elementToValidate.Name == "Foo")
            context.LogError("Name cannot be Foo!", "ErrExt_001", elementToValidate);
         if (elementToValidate.Name == "Bar")
            context.LogWarning("Name should not be Bar!", "WarnExt_001", elementToValidate);
      }
   }
}
