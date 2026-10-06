using backend_claro.Domain.Entities;
using backend_claro.Application.DTOs;
using backend_claro.Application.DTOs.Adicional;
namespace backend_claro.Application.Mappings;

public static class AdicionalMapper
{
    public static CostoAdicional ToEntity (this CrearAdicionalDto request)
    {
        return new CostoAdicional
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Precio = request.Precio,
            Tipo = request.Tipo,
            ColumnaExcel = request.ColumnaExcel ?? String.Empty

        };
    }

        public static RequestAdicionalDto ToResponse (this CostoAdicional request)
    {
        return new RequestAdicionalDto
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Precio = request.Precio,
            Tipo = request.Tipo,
            ColumnaExcel = request.ColumnaExcel ?? String.Empty

        };
    }

}