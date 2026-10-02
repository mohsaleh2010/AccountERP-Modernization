using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib 
{
    public partial  class Common
    {
        public static int ParseInt(string input, int fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(input))
                return fallback;

            // 1. Replace Unicode dashes (en-dash, em-dash, Arabic minus, figure dash) with ASCII '-'
            string sanitized = input
                .Replace('\u2013', '-') // En-dash
                .Replace('\u2014', '-') // Em-dash
                .Replace('\u2212', '-') // Minus sign
                .Replace('\u200E', ' ') // LRM (Left-to-Right Mark)
                .Replace('\u200F', ' ') // RLM (Right-to-Left Mark)
                .Trim();

            // 2. Parse using InvariantCulture and standard integer styles (allows leading sign & whitespace)
            if (int.TryParse(sanitized,
                             System.Globalization.NumberStyles.Integer,
                             System.Globalization.CultureInfo.InvariantCulture,
                             out int result))
            {
                return result;
            }

            return fallback;
        }
    }
}
