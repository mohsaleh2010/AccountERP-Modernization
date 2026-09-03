using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    public class ErrorResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public string target { get; set; }

    }
}
