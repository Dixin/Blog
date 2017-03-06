namespace Tutorial.ParallelLinq
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public struct QuerySettings
    {
        public TaskScheduler TaskScheduler { get; set; }

        public int? DegreeOfParallelism { get; set; }

        public ParallelExecutionMode? ExecutionMode { get; set; }

        public ParallelMergeOptions? MergeOptions { get; set; }
    }

    public static partial class ParallelEnumerableX
    {
        private static readonly PropertyInfo QuerySettingsProperty = typeof(ParallelQuery).GetTypeInfo().GetProperty(
            "SpecifiedQuerySettings", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

        private static readonly Type QuerySettingsType = typeof(ParallelQuery).GetTypeInfo().Assembly.GetType(
            "System.Linq.Parallel.QuerySettings");

        private static readonly PropertyInfo TaskSchedulerProperty = QuerySettingsType.GetTypeInfo().GetProperty(
            "TaskScheduler", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

        private static readonly PropertyInfo DegreeOfParallelismProperty = QuerySettingsType.GetTypeInfo().GetProperty(
            "DegreeOfParallelism", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

        private static readonly PropertyInfo ExecutionModeProperty = QuerySettingsType.GetTypeInfo().GetProperty(
            "ExecutionMode", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

        private static readonly PropertyInfo MergeOptionsProperty = QuerySettingsType.GetTypeInfo().GetProperty(
            "MergeOptions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

        public static QuerySettings SpecifiedQuerySettings(this ParallelQuery source)
        {
            object querySettings = QuerySettingsProperty.GetValue(source);
            return new QuerySettings()
                {
                    TaskScheduler = (TaskScheduler)TaskSchedulerProperty.GetValue(querySettings),
                    DegreeOfParallelism = (int?)DegreeOfParallelismProperty.GetValue(querySettings),
                    ExecutionMode = (ParallelExecutionMode?)ExecutionModeProperty.GetValue(querySettings),
                    MergeOptions = (ParallelMergeOptions?)MergeOptionsProperty.GetValue(querySettings)
                };
        }
    }
}