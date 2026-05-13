using System;

namespace XCharts.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DefaultAnimationAttribute : Attribute
    {
        public readonly bool enableSerieDataAddedAnimation = true;
        public readonly AnimationType type;

        public DefaultAnimationAttribute(AnimationType handler)
        {
            type = handler;
        }

        public DefaultAnimationAttribute(AnimationType handler, bool enableSerieDataAddedAnimation)
        {
            type = handler;
            this.enableSerieDataAddedAnimation = enableSerieDataAddedAnimation;
        }
    }
}