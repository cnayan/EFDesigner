﻿namespace Sawczyn.EFDesigner.EFModel
{
   public static class StringExtensions
   {
      public static string Truncate(this string text, int length, string ellipsis = "...", bool keepFullWordAtEnd = true)
      {

         if (string.IsNullOrEmpty(text))
            return string.Empty;

         if (text.Length < length)
            return text;

         string result = text.TrimEnd().Substring(0, length);

         if (keepFullWordAtEnd && result.IndexOf(' ') >= 0)
            result = result.Substring(0, result.LastIndexOf(' '));

         return result + ellipsis;
      }
   }
}
