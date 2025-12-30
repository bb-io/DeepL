using Tests.DeepL.Base;
using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Tests.DeepL;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task GlossariesDataHandler_ReturnsGlossaries()
    {
        // Arrange
        var handler = new GlossariesDataHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        // Assert
        foreach (var item in result)
            Console.WriteLine($"{item.Value} - {item.DisplayName}");
        Assert.IsNotNull(result);
    }
}
