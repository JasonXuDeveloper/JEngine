using YooAsset;

public class RemoteServices : IRemoteServices
{
    private string _resourceUrl;
    private string _fallbackUrl;

    public RemoteServices(string resourceUrl, string fallbackUrl)
    {
        _resourceUrl = resourceUrl;
        _fallbackUrl = fallbackUrl;
    }

    public string GetRemoteMainURL(string fileName)
    {
        return _resourceUrl + fileName;
    }

    public string GetRemoteFallbackURL(string fileName)
    {
        return _fallbackUrl + fileName;
    }
}