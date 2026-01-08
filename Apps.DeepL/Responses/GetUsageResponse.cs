using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL.Responses;

public class GetUsageResponse
{

      [Display("Characters used in current billing period")]
      public long? CharacterCount { get; set; }
      
      [Display("Character limit per billing period")]
      public long? CharacterLimit { get; set; }
      
      [Display("Character limit reached?")]
      public bool CharacterLimitReached { get; set; }

      [Display("Documents translated current billing period")]     
      public long? DocumentCount { get; set; }
      
      [Display("Document limit per billing period")]
      public long? DocumentLimit { get; set; }
      
      [Display("Document limit reached?")]
      public bool DocumentLimitReached { get; set; }

      [Display("Team Documents translated current billing period")]
      public long? TeamDocumentCount { get; set; }
       
      [Display("Team Document limit per billing period?")]
      public long? TeamDocumentLimit { get; set; }

      [Display("Team Document limit reached?")]
      public bool TeamDocumentLimitReached { get; set; }
}