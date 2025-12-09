using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace Dominio.DTOs

{

    public record RegistrarAtencionDto(

     Guid IngresoId,

     Guid MedicoId,

     string Informe

);

}
