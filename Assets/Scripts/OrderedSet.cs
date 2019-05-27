using System.Collections;
using System.Collections.Generic;

public class OrderedSet<T>
{
    private readonly HashSet<T> m_HashSet;
    private readonly IList<T> m_List;

    private static System.Random rnd = new System.Random(42);

    public int Count
    {
        get
        {
            return m_HashSet.Count;
        }
    }

    public OrderedSet()
    {
        m_HashSet = new HashSet<T>();
        m_List = new List<T>();
    }

    public void Add(T val)
    {
        m_HashSet.Add(val);
        m_List.Insert(m_List.Count, val);
        // m_List.AddLast(val);
    }

    public void Remove(T val)
    {
        if (!m_HashSet.Contains(val)) return;

        m_HashSet.Remove(val);
        m_List.Remove(val);
    }

    public bool Contains(T val)
    {
        return m_HashSet.Contains(val);
    }

    public T Next()
    {
        if (m_HashSet.Count == 0) return default(T);  // eye: is it null?

        T first = m_List[0];
        // m_List.RemoveFirst();
        // m_HashSet.Remove(first);
        return first;
    }

    public T NextRnd()
    {
        if (m_HashSet.Count == 0) return default(T);

        return m_List[rnd.Next(m_List.Count)];
    }
}
