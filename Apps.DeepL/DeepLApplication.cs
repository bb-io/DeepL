using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL;

public class DeepLApplication : IApplication
{
    public string Name
    {
        get => "DeepL";
        set { }
    }

    public IPublicApplicationMetadata? PublicApplicationMetadata => throw new NotImplementedException();

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}