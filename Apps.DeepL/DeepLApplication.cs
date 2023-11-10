using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL;

public class DeepLApplication : IApplication
{
    public string Name
    {
        get => "DeepL";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}