using System.Collections.ObjectModel;
using System.Fabric.Description;

namespace UserOrchestrator
{
    internal class ServiceMetric : KeyedCollection<string, ServiceLoadMetricDescription>
    {
        protected override string GetKeyForItem(ServiceLoadMetricDescription item)
        {
            return item.Name;
        }
    }
}