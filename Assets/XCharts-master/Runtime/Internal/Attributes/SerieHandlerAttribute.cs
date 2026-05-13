using System;

namespace XCharts.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SerieHandlerAttribute : Attribute
    {
        public readonly bool allowMultiple = true;
        public readonly Type handler;

        public SerieHandlerAttribute(Type handler)
        {
            this.handler = handler;
            allowMultiple = true;
        }

        public SerieHandlerAttribute(Type handler, bool allowMultiple)
        {
            this.handler = handler;
            this.allowMultiple = allowMultiple;
        }
    }
}