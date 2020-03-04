//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
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
using System.Data.Entity.Spatial;

namespace Testing
{
   /// <summary>
   /// x
   /// </summary>
   public partial class UParentOptional: global::Testing.HiddenEntity, INotifyPropertyChanged
   {
      partial void Init();

      /// <summary>
      /// Default constructor. Protected due to required properties, but present because EF needs it.
      /// </summary>
      protected UParentOptional(): base()
      {
         PropertyInChild = "hello";
         UChildCollection = new System.Collections.ObjectModel.ObservableCollection<global::Testing.UChild>();

         Init();
      }

      /// <summary>
      /// Public constructor with required data
      /// </summary>
      /// <param name="uchildrequired"></param>
      public UParentOptional(global::Testing.UChild uchildrequired)
      {
         this.PropertyInChild = "hello";
         if (uchildrequired == null) throw new ArgumentNullException(nameof(uchildrequired));
         this.UChildRequired = uchildrequired;

         this.UChildCollection = new System.Collections.ObjectModel.ObservableCollection<global::Testing.UChild>();

         Init();
      }

      /// <summary>
      /// Static create function (for use in LINQ queries, etc.)
      /// </summary>
      /// <param name="uchildrequired"></param>
      public static UParentOptional Create(global::Testing.UChild uchildrequired)
      {
         return new UParentOptional(uchildrequired);
      }

      /*************************************************************************
       * Properties
       *************************************************************************/

      /// <summary>
      /// Backing field for PropertyInChild
      /// </summary>
      protected string _PropertyInChild;
      /// <summary>
      /// When provided in a partial class, allows value of PropertyInChild to be changed before setting.
      /// </summary>
      partial void SetPropertyInChild(string oldValue, ref string newValue);
      /// <summary>
      /// When provided in a partial class, allows value of PropertyInChild to be changed before returning.
      /// </summary>
      partial void GetPropertyInChild(ref string result);

      /// <summary>
      /// Default value = "hello"
      /// </summary>
      public string PropertyInChild
      {
         get
         {
            string value = _PropertyInChild;
            GetPropertyInChild(ref value);
            return (_PropertyInChild = value);
         }
         set
         {
            string oldValue = _PropertyInChild;
            SetPropertyInChild(oldValue, ref value);
            if (oldValue != value)
            {
               _PropertyInChild = value;
            }
         }
      }

      /*************************************************************************
       * Navigation properties
       *************************************************************************/

      /// <summary>
      /// x
      /// </summary>
      public virtual global::Testing.UChild UChildOptional { get; set; }

      /// <summary>
      /// h
      /// </summary>
      public virtual ICollection<global::Testing.UChild> UChildCollection { get; protected set; }

      /// <summary>
      /// Required
      /// </summary>
      public virtual global::Testing.UChild UChildRequired { get; set; }

      public virtual event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

   }
}

