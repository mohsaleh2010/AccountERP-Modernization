// Decompiled with JetBrains decompiler
// Type: E_Invoice_System.Api.ApiModels.GetDocumentModel
// Assembly: electronic invoice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB5BCC06-2FA2-486D-AF6E-E5642E35302C
// Assembly location: C:\Program Files (x86)\electronic invoice\electronic invoice\electronic invoice.exe

using Newtonsoft.Json;
 

namespace AccountModelsLib.Models
{
  public class GetDocumentModel
  {
    private string validation;
    private string document1;

    public string document
    {
      get => this.document1;
      set
      {
        this.document1 = value;
        this.Documents = JsonConvert.DeserializeObject<Document>(value.Replace("\\\"", "\""));
      }
    }

    public ValidationResults validationResults { get; set; }

    public string uuid { get; set; }

    public string submissionUUID { get; set; }

    public string status { get; set; }

    public Document Documents { get; set; }

    public string Validation
    {
      get
      {
        this.validation = string.Empty;
        if (this.validationResults == null)
          return this.validation;
        this.validation = "Status :" + this.validationResults.status + "\n\n";
        foreach (ValidationStep validationStep in this.validationResults.validationSteps)
          this.validation += string.Format("֎ {0}  Status: {1}\n{2}\n", (object) validationStep.name, (object) validationStep.status, validationStep.error);
        return this.validation.Replace("Validator", "");
      }
    }
  }
}
