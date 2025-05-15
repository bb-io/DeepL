using Apps.DeepL.Actions;
using Apps.DeepL.Models;
using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.DeepL.Base;

namespace Tests.DeepL
{
    [TestClass]
    public class GlossaryActionsTests :TestBase
    {
        [TestMethod]
        public async Task ImportGlossaryV3_IssSuccess()
        {
            var action = new GlossaryActions(InvocationContext, FileManager);

            var result = await action.ImportGlossaryV3(new ImportGlossaryRequest {
                File = new FileReference { Name= "Original - Copy.tbx" } });


            Console.WriteLine(result.GossaryId);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ExportGlossary_IssSuccess()
        {
            var action = new GlossaryActions(InvocationContext, FileManager);

            var result = await action.ExportGlossary(new GlossaryRequest { GlossaryId= "75266ac6-fafa-43e4-8f2d-4520feb7b379",

            });

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdatetGlossaryV3_IssSuccess()
        {
            var action = new GlossaryActions(InvocationContext, FileManager);

            var result = await action.AddOrReplaceDictionaryV3(new UpdateGlossaryRequest
            {
                SourceLanguageCode = "en",
                TargetLanguageCode = "nl",
                GlossaryId = "2cbf92b3-e7bd-4538-8a7c-0d5dfcac4778",
                File = new FileReference { Name = "Original - Copy.tbx" }
            });

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ExportGlossaryv3_IssSuccess()
        {
            var action = new GlossaryActions(InvocationContext, FileManager);

            var result = await action.ExportGlossaryV3(new GlossaryRequest
            {
                GlossaryId = "2cbf92b3-e7bd-4538-8a7c-0d5dfcac4778",

            });
            Assert.IsNotNull(result);
        }
    }
}
