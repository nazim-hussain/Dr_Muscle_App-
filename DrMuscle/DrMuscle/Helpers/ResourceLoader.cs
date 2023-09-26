using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Linq;
namespace DrMuscle.Helpers
{
    public class ResourceLoader
    {
public ResourceLoader(ResourceManager resourceManager)
    {
        this.manager = resourceManager;
        Instance = this;
        this.cultureInfo = CultureInfo.CurrentUICulture;
    }

    private readonly ResourceManager manager;
    private CultureInfo cultureInfo;

    private readonly List<StringResource> resources = new List<StringResource>();

    public static ResourceLoader Instance { get; private set; }

    public StringResource this[string key] {
        get { return this.GetString(key); }
    }

    public StringResource GetString(string resourceName)
    {
        string stringRes = this.manager.GetString(resourceName, this.cultureInfo);
        var stringResource = new StringResource(resourceName, stringRes);
        var res = this.resources.Where(x => x.Key == stringResource.Key).FirstOrDefault();
        if (res != null)
            return res;
        this.resources.Add(stringResource);
        return stringResource;
    }

    public void SetCultureInfo(CultureInfo cultureInfo)
    {
        this.cultureInfo = cultureInfo;
        foreach (StringResource stringResource in this.resources) {
            stringResource.Value = this.manager.GetString(stringResource.Key, cultureInfo);
        }
    }
    }
}
