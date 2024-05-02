using Blackbird.Applications.Sdk.Common;
using DeepL.Model;

namespace Apps.DeepL.Entities;

public class GlossaryEntity
{
    [Display("Glossary ID")] public string GlossaryId { get; set; }

    [Display("Ready")] public bool Ready { get; set; }

    [Display("Name")] public string Name { get; set; }

    [Display("Source language")] public string SourceLang { get; set; }

    [Display("Target language")] public string TargetLang { get; set; }

    [Display("Creation time")] public DateTime CreationTime { get; set; }

    [Display("Entry count")] public int EntryCount { get; set; }

    public GlossaryEntity(GlossaryInfo glossary)
    {
        GlossaryId = glossary.GlossaryId;
        Ready = glossary.Ready;
        Name = glossary.Name;
        SourceLang = glossary.SourceLanguageCode;
        TargetLang = glossary.TargetLanguageCode;
        CreationTime = glossary.CreationTime;
        EntryCount = glossary.EntryCount;
    }
}