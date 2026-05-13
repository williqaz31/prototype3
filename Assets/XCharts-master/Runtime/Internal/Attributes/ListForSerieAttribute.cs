using System;

namespace XCharts.Runtime
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ListForSerie : ListFor
    {
        public ListForSerie(Type type) : base(type)
        {
        }
    }
}