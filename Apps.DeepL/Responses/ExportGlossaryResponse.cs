﻿using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Responses;

public class ExportGlossaryResponse
{
    [Display("Glossary")]
    public FileReference File { get; set; }
}