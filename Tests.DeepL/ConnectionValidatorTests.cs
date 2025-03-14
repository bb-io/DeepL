using Apps.DeepL.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    private ConnectionValidator _validator;

    [TestInitialize]
    public void Setup()
    {
        _validator = new ConnectionValidator();
    }

    [TestMethod]
    public async Task ValidateConnection_WithValidCredentials_ReturnsValid()
    {
        var result = await _validator.ValidateConnection(Creds, CancellationToken.None);
        
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_WithInvalidCredentials_ReturnsInvalid()
    {
        var invalidCreds = Creds.Select(x => 
            new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));
            
        var result = await _validator.ValidateConnection(invalidCreds, CancellationToken.None);
        
        Assert.IsFalse(result.IsValid);
    }
}