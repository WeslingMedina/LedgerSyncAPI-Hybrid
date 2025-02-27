using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ApiResponseDto(
        int Status,
        string To,
        List<string>? Text
    );
}
