//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor
//     https://github.com/msawczyn/EFDesigner
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sandbox_EF6
{
   public partial class Detail: global::Sandbox_EF6.BaseClass
   {
      partial void Init();

      /// <summary>
      /// Default constructor. Protected due to required properties, but present because EF needs it.
      /// </summary>
      protected Detail(): base()
      {
         BaseClasses = new System.Collections.Generic.HashSet<global::Sandbox_EF6.BaseClass>();

         Init();
      }

      /// <summary>
      /// Public constructor with required data
      /// </summary>
      /// <param name="_detail0"></param>
      public Detail(global::Sandbox_EF6.Detail _detail0)
      {
         if (_detail0 == null) throw new ArgumentNullException(nameof(_detail0));
         _detail0.BaseClasses.Add(this);

         this.BaseClasses = new System.Collections.Generic.HashSet<global::Sandbox_EF6.BaseClass>();
         Init();
      }

      /// <summary>
      /// Static create function (for use in LINQ queries, etc.)
      /// </summary>
      /// <param name="_detail0"></param>
      public static Detail Create(global::Sandbox_EF6.Detail _detail0)
      {
         return new Detail(_detail0);
      }

      /*************************************************************************
       * Properties
       *************************************************************************/

      public string StringMax { get; set; }

      /*************************************************************************
       * Navigation properties
       *************************************************************************/

      public virtual ICollection<global::Sandbox_EF6.BaseClass> BaseClasses { get; protected set; }

   }
}

