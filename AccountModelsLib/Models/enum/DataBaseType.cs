using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace AccountModelsLib.Models
{
    public enum DataBaseType
    {
        SqlServer,
        Oracle,
        Acess,
        None,
    }
}
