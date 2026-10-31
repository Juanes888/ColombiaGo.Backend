namespace ColombiaGo.Application.DTOs.Guias;

/// <summary>Mirrors the frontend's <c>Guide</c> type in <c>src/data/guides.ts</c>.</summary>
public sealed record GuiaDto(
    int Id,
    string Name,
    string Specialty,
    string Tag,
    string TagTone,
    string Place,
    decimal Rating,
    int Reviews,
    string Price,            // "$70.000 por hora"
    IReadOnlyList<string> Languages,
    string? Image,
    bool Featured,
    string? Badge,
    bool Verified,
    string? Bio,             // presentación en el idioma de la petición (cae a español si no hay traducción)
    int ExperienceYears,
    int ToursCount);
