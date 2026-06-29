using System.Collections.Generic;
using _Game.Scripts.Utils.VContainer;
using SpektraGames.SpektraUtilities.Runtime;

namespace Analytics
{
    public interface IReportableService : IService
    {
        InfoLogger InfoLogger { get; }
        public void ReportEvent(string eventName);
        public void ReportEvent(string eventName, Dictionary<string, AnalyticsEventParameter> paramsToSend);
        public void ReportErrorEvent(string eventName, string errorType, string errorMessage, string exceptionMessage, string details);
    }
}
