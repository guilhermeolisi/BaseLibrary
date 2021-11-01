using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GosControls.Models
{
    public static class RegExProjects
    {
        public static readonly Regex reChangeProperty = new Regex(@"^\s*(?<Property>\S+)\s+(?<Value>[^\r\n]+)?(?>\r\n|\r|\n|\z)", RegexOptions.Multiline | RegexOptions.ExplicitCapture);
        public static readonly Regex reTitle = new Regex(@"[^\r\n]*", RegexOptions.Multiline | RegexOptions.ExplicitCapture);


    }
}
