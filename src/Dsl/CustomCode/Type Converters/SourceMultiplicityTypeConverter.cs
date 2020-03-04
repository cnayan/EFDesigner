﻿using Microsoft.VisualStudio.Modeling.Diagrams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Sawczyn.EFDesigner.EFModel
{
   public class SourceMultiplicityTypeConverter : MultiplicityTypeConverter
   {
      /// <summary>Converts the given value object to the specified type, using the specified context and culture information.</summary>
      /// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <param name="culture">
      ///    A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is
      ///    assumed.
      /// </param>
      /// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
      /// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to. </param>
      /// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType" /> parameter is null. </exception>
      /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
      public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
      {
         if (context != null)
         {
            Association association = context.Instance as Association;

            if (association == null && context.Instance is AssociationConnector connector)
               association = PresentationViewsSubject.GetSubject(connector) as Association;

            if (destinationType == typeof(string) && association != null && value is Multiplicity multiplicity)
            {
               switch (multiplicity)
               {
                  case Multiplicity.One:
                     return $"1 (One {association.Source.Name})";
                  //case Multiplicity.OneMany:
                  //   return $"1..* (Collection of one or more {association.Source.Name})";
                  case Multiplicity.ZeroMany:
                     return $"* (Collection of {association.Source.Name})";
                  case Multiplicity.ZeroOne:
                     return $"0..1 (Zero or one of {association.Source.Name})";
               }
            }
         }

         return base.ConvertTo(context, culture, value, destinationType);
      }

      /// <summary>
      ///    Returns a collection of standard values for the data type this type converter is designed for when provided
      ///    with a format context.
      /// </summary>
      /// <returns>
      ///    A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of
      ///    valid values, or null if the data type does not support a standard set of values.
      /// </returns>
      /// <param name="context">
      ///    An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context
      ///    that can be used to extract additional information about the environment from which this converter is invoked. This
      ///    parameter or properties of this parameter can be null.
      /// </param>
      public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
         Association association = context.Instance as Association;

         if (association == null && context.Instance is AssociationConnector connector)
            association = PresentationViewsSubject.GetSubject(connector) as Association;

         if (association == null)
            return new StandardValuesCollection(new string[0]);

         List<string> result = new List<string>
                                {
                                    $"* (Collection of {association.Source.Name})",
                                    //$"1..* (Collection of one or more {association.Source.Name})",
                                    $"0..1 (Zero or one of {association.Source.Name})",
                                    $"1 (One {association.Source.Name})"
                                };
         return new StandardValuesCollection(result);
      }
   }
}
