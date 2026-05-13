using System;

namespace XCharts.Runtime
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ListForComponent : ListFor
    {
        public ListForComponent(Type type) : base(type)
        {
        }
    }
}