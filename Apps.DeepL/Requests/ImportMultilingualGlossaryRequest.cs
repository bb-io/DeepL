using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Requests;

public class ImportMultilingualGlossaryRequest
{
    [Display("Glossary", Description = "Glossary file exported from other Blackbird apps")]
    public FileReference File {  get; set; }

    [Display("New glossary name", Description = "You can override glossary name which is set in a file")]
    public string? Name { get; set; }
}