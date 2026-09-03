// Decompiled with JetBrains decompiler
// Type: InternalDatabase.Models.Branch
// Assembly: InternalDatabase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A754F9EC-D0E5-48B4-96C2-614D8440FE1D
// Assembly location: E:\الفاتورة الالكترونية\E_Invoice_System\dll\InternalDatabase.dll

using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    [Table("Tax_Branches")]
  public class Branch : Base
  {
    public string Name { get; set; }

    public string TaxNumber { get; set; }

    public string RegistrationNumber { get; set; }

    public string Address { get; set; }

    public string BranchID { get; set; } = "0";

    public string Governate { get; set; } = "القاهرة";

    public string RegionCity { get; set; } = "مدينة";

    public string Street { get; set; } = "الشارع";

    public string Phone1 { get; set; }

    public string Phone2 { get; set; }

    public string Phone3 { get; set; }

    public string Fax { get; set; }

    public string Email { get; set; }

    public string BankName { get; set; }

    public string AccountNo { get; set; }

    public string IBANNumber { get; set; }

    public string BankAddress { get; set; }

    public bool Active { get; set; }

    public byte[] Logo { get; set; }
  }
}
