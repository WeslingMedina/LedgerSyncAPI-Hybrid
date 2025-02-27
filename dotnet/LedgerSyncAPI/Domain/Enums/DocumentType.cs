using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum DocumentType
    {
        FacturaElectronica = 1,
        NotaDebitoElectronica = 2,
        NotaCreditoElectronica = 3,
        TiqueteElectronico = 4,
        MensajeReceptor = 5
    }
}
