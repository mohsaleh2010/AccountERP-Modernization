// Decompiled with JetBrains decompiler
// Type: InternalDatabase.Models.Base
// Assembly: InternalDatabase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A754F9EC-D0E5-48B4-96C2-614D8440FE1D
// Assembly location: E:\الفاتورة الالكترونية\E_Invoice_System\dll\InternalDatabase.dll

namespace AccountModelsLib.Models
{
  public class Base
  {
    public long Id { get; set; }
        public long? UserId { get; set; }
        public RowStatus RowStatus { get; set; }

    public string RegistrationTaxFlag { get; set; }

    
  }
}
