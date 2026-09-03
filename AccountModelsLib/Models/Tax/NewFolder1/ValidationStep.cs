// Decompiled with JetBrains decompiler
// Type: E_Invoice_System.Api.ApiModels.ValidationStep
// Assembly: electronic invoice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB5BCC06-2FA2-486D-AF6E-E5642E35302C
// Assembly location: C:\Program Files (x86)\electronic invoice\electronic invoice\electronic invoice.exe

namespace AccountModelsLib.Models
{
  public class ValidationStep
  {
    public string name { get; set; }

    public string status { get; set; }

    public object error { get; set; }
  }
}
