 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace AccountModelsLib.Models
{
    public class AccessTokenResposnse
  {
    public string access_token { get; set; }

    public string expires_in { get; set; }

    public string token_type { get; set; }

    public string scope { get; set; }
  }
}
