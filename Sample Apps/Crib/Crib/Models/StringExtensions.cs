using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Crib.Models
{
    public static class StringExtensions
    {
        public static DateTime Parse(this string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return DateTime.Today;

            return DateTime.Parse(date);
        }
    }
}