using System.Collections.Generic;

namespace XCharts.Runtime
{
    public static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> s_ListPool = new(OnGet, OnClear);

        private static void OnGet(List<T> l)
        {
            if (l.Capacity < 50) l.Capacity = 50;
        }

        private static void OnClear(List<T> l)
        {
            l.Clear();
        }

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }

        public static void ClearAll()
        {
            s_ListPool.ClearAll();
        }
    }
}