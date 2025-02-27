using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum TipoDocReferencia { _01, _02, _03, _04, _05, _06, _07, _08, _09, _99 }
    public enum CodigoReferencia { _01, _02, _04, _05, _99 }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TipoIdentificacion
    {
        [EnumMember(Value = "01")]
        CedulaFisica = 1,

        [EnumMember(Value = "02")]
        CedulaJuridica = 2,

        [EnumMember(Value = "03")]
        DIMEX = 3,

        [EnumMember(Value = "04")]
        NITE = 4,

        [EnumMember(Value = "05")]
        IdentificacionExtranjera = 5
    }
}
