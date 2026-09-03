// Decompiled with JetBrains decompiler
// Type: E_Invoice_System.Api.ApiModels.ValidationResults
// Assembly: electronic invoice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB5BCC06-2FA2-486D-AF6E-E5642E35302C
// Assembly location: C:\Program Files (x86)\electronic invoice\electronic invoice\electronic invoice.exe

using System.Collections.Generic;

namespace AccountModelsLib.Models
{
  public class ValidationResults
  {
    public string status { get; set; }

    public List<ValidationStep> validationSteps { get; set; }
  }
}
