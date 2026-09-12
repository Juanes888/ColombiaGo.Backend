using ColombiaGo.Application.DTOs.Categorias;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class CategoriaMappings
{
    public static CategoriaDto ToDto(this Categoria categoria) =>
        new(categoria.Slug, categoria.Nombre, categoria.Icono, categoria.Tono);
}
