using System.Runtime.Serialization;

namespace API_CONSULTATION.Application.Enums
{
    public enum FormaPagoEnum
    {
        [EnumMember(Value = "Efectivo")]
        Efectivo = 1,

        [EnumMember(Value = "TDC")]
        TDC = 2,

        [EnumMember(Value = "TDD")]
        TDD = 3,
    }
}
