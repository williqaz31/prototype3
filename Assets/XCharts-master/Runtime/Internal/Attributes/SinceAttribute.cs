using System;

namespace XCharts.Runtime
{
    [AttributeUsage(AttributeTargets.All)]
    public class Since : Attribute
    {
        public readonly string version;

        public Since(string version)
        {
            this.version = version;
        }
    }
}