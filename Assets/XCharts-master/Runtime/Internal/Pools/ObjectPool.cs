using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XCharts.Runtime
{
    public class ObjectPool<T> where T : new()
    {
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;
        private readonly bool m_NewIfEmpty = true;
        private readonly Stack<T> m_Stack = new();

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease, bool newIfEmpty = true)
        {
            m_NewIfEmpty = newIfEmpty;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public int countAll { get; private set; }
        public int countActive => countAll - countInactive;
        public int countInactive => m_Stack.Count;

        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                if (!m_NewIfEmpty) return default;
                element = new T();
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }

            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }

        public void ClearAll()
        {
            m_Stack.Clear();
        }
    }
}