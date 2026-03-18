namespace Authentication.Application.Manager;

public interface IFeatureFlagManager
{
    Task<bool> IsEnabledAsync(string feature);
}