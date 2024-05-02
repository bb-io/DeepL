using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.DeepL;

public class DeepLApplication : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.MachineTranslationAndMtqe];
        set { }
    }
    
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