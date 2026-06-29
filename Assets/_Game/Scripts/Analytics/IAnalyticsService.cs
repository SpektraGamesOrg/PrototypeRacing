using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Analytics
{
    public interface IAnalyticsService : IReportableService
    {
        bool IsInitialized { get; }
        UniTask InitializeAsync();
        void SetUserId(string userId);
        void SetUserProperty(string name, string property);
    }
}
