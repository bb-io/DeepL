# Blackbird.io DeepL

Blackbird is the new automation backbone for the language technology industry.
Blackbird provides enterprise-scale automation and orchestration with a simple
no-code/low-code platform. Blackbird enables ambitious organizations to
identify, vet and automate as many processes as possible. Not just localization
workflows, but any business and IT process. This repository represents an
application that is deployable on Blackbird and usable inside the workflow
editor.

## Introduction

<!-- begin docs -->

DeepL is an artificial intelligence (AI) company that specializes in language
translation services. It offers a neural machine translation (NMT) engine
capable of providing high-quality translations across multiple languages.
DeepL's translation engine is known for its accuracy, natural-sounding
translations, and ability to understand context.

## Before setting up

Before you can connect you need to make sure that:

- You have a DeepL account.
- You have an API key for your DeepL account. It can be found under _account
  settings_ in DeepL.
- **Important**: If you are using an API key for CAT tools, it will not work for
  the public API. Instead, you need to use the Authentication key for DeepL API.
  You can find [guidance](https://youtu.be/WTt3UuiDAf4?t=79) on where it is
  located in this guide. For more details, you can also refer to this
  [discussion](https://github.com/DeepLcom/deepl-python/issues/106), where a
  similar issue was resolved.

## Connecting

1. Navigate to Apps, and identify the DeepL app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My DeepL connection'.
4. Fill in the API key to your DeepL account.
5. Click _Connect_.

![DeepLBlackbirdConnection](image/README/DeepLBlackbirdConnection.png)

## Actions

### Translation

- **Translate** Translate interopability compatible files in Blackbird
  interoperability mode. Can also be used to translate other file types that
  DeepL supports. If you're only translating text (strings) then use _Translate
  text_ instead. Advanced settings:
  - **File translation strategy**: Select whether to use DeepL's own file
    processing capabilities or use Blackbird interoperability mode with DeepL's
    more advanced models (the latter the default mode).
  - **Output file handling**: If using Blackbird's interoperability mode, this
    determines the format of the output file. The default Blackbird behavior is
    to convert to XLIFF for future steps. You can change it to output the
    original file format (if you don't want to continue language operationts
    after this step).
  - **Glossary ID**: Select the DeepL glossary you want to use for this
    translation.
  - **Context**: Add additional context to the translation, this can be anything
    of relevance.
  - **Preserve formatting**: Whether to preserve the text formatting during
    translation.
  - **Formality**: Indicates whether the translation should be formal (depends
    on the language).
  - **Model type**: Specifies which DeepL model should be used for translation.
    You can choose between speed and quality here.
- **Translate text** Translate a single text string. Useful when translating
  small messages. For larger content and files use _Translate_ instead.

To be deprecated soon:

- **Translate XLIFF** Translate an XLIFF file using the text translation
  endpoint. Useful when using the next-generation model for small XLIFF files
  (support only 1.2 version). Supported file extensions: `.xliff`, `.xlf`,
  `.mqxliff`, `.mxliff`, `.txlf`. This action will soon be deprecated once
  Blackbird interoperability mode also supports all these dialects.

### Write

- **Improve text** improve a text using DeepL Write, set writing style or tone.
  Currently only supports some of the languages. Can also be used to change
  locale from f.e. American English to British English. For more details see
  [DeepL's write documentation](https://developers.deepl.com/docs/api-reference/improve-text).

### Glossaries

- **Export glossary** Export glossary
- **Import glossary** Import glossary (.tbx, .csv & .tsv)
- **Get glossary details** Get details of a specific glossary
- **Get glossary entries** Get glossary entries in a TSV format
- **Search glossaries** List all glossaries
- **Delete glossary** Delete a glossary
- **Import glossary (multilingual)** Import a glossary with multiple language
  pairs.
- **Update dictionary (multilingual)** Updates multilingual dictionary
- **Export glossary (multilingual)** Export multilingual glossary

## Example

![DeepLExample](image/README/DeepLExample.png)

The example above shows a bird that is triggered as soon as an article is
published in Zendesk, said article is then exported as an HTML document and
translated through DeepL before being imported back into Zendesk. The DeepL
translation also considers as guardrail a Glossary that has been exported from
Microsoft Excel.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach
out to us using the [established channels](https://www.blackbird.io/) or create
an issue.

<!-- end docs -->
