using TierMatch.Domain.Common;

namespace TierMatch.Domain.Entities;

public class AnimalImage : BaseEntity
{
    /// <summary>
    /// Zugehöriges Tier.
    /// </summary>
    public Guid AnimalId { get; set; }

    public Animal Animal { get; set; } = null!;

    /// <summary>
    /// Ursprünglicher Dateiname.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Speicherpfad der Datei.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// MIME-Type (image/jpeg, image/png ...)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Dateigröße in Byte.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Titelbild.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Reihenfolge der Bilder.
    /// </summary>
    public int SortOrder { get; set; }
}