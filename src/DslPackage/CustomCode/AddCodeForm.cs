using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sawczyn.EFDesigner.EFModel.DslPackage.CustomCode
{
   public partial class AddCodeForm : Form
   {
      private readonly ModelRoot modelRoot;
      private string _parserError = null;
      
      public AddCodeForm()
      {
         InitializeComponent();
         txtCode.AutoCompleteCustomSource.AddRange(ModelAttribute.ValidTypes);
      }

      public AddCodeForm(ModelClass element, ModelRoot elementModelRoot, string errorMessage) : this()
      {
         modelRoot = elementModelRoot;
         lblClassName.Text = element.Name;
         txtCode.Lines = element.Attributes.Select(x => $"{x};").ToArray();
      }

      public IEnumerable<string> Lines => txtCode.Lines;

      private void btnOk_Click(object sender, EventArgs e)
      {
         DialogResult = DialogResult.OK;
      }

      public string ParserError
      {
         get { return _parserError; }
      }
      
      public IEnumerable<ModelAttribute.ParseResult> ParseResults
      {
         get
         {
            if (modelRoot == null)
               return null;

            lblErrorMessage.Text = string.Empty;
            btnOk.Enabled = true;

            List<ModelAttribute.ParseResult> parseResults = Lines.Select(s => ModelAttribute.Parse(modelRoot, s)).ToList();
            foreach (ModelAttribute.ParseResult t in parseResults.Where(t => !string.IsNullOrEmpty(t.ErrorMessage))) 
            {
               lblErrorMessage.Text = t.ErrorMessage;
               btnOk.Enabled = false;
               txtCode.SelectionStart = txtCode.Text.IndexOf(t.SourceText, StringComparison.Ordinal);
               txtCode.SelectionLength = 0;
               return null;
            }

            return parseResults;
            //ModelAttribute.ParseResult firstError = parseResults.FirstOrDefault(r => !string.IsNullOrEmpty(r.ErrorMessage));

         }
      }
   }
}
