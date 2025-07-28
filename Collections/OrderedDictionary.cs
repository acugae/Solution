namespace Solution.Collections;

public class OrderedDictionary<TKey, TValue> : OrderedDictionary, IEnumerable<KeyValuePair<TKey, TValue>>
{
    public OrderedDictionary() : base() { }

    // Costruttore con capacità iniziale
    public OrderedDictionary(int capacity) : base(capacity) { }
    // Aggiunge un elemento con controllo del tipo
    public void Add(TKey key, TValue value)
    {
        base.Add(key, value);
    }

    // Ottiene il valore con cast automatico
    public new TValue this[object key]
    {
        get => (TValue)base[key];
        set => base[key] = value;
    }

    // Ottiene il valore per indice con cast automatico
    public new TValue this[int index]
    {
        get => (TValue)base[index];
        set => base[index] = value;
    }

    // Metodo per ottenere il valore in modo sicuro
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (Contains(key))
        {
            value = (TValue)base[key];
            return true;
        }
        value = default(TValue);
        return false;
    }

    // Ottiene tutte le chiavi tipizzate
    public IEnumerable<TKey> Keys
    {
        get
        {
            foreach (TKey key in base.Keys)
            {
                yield return key;
            }
        }
    }

    // Ottiene tutti i valori tipizzati
    public IEnumerable<TValue> Values
    {
        get
        {
            foreach (TValue value in base.Values)
            {
                yield return value;
            }
        }
    }

    // Metodo per ottenere le coppie chiave-valore tipizzate
    public IEnumerable<KeyValuePair<TKey, TValue>> AsKeyValuePairs()
    {
        foreach (var entry in this)
        {
            yield return new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
        }
    }

    // Implementazione del metodo Copy
    public OrderedDictionary<TKey, TValue> Copy()
    {
        var capacity = this.Count;
        var copy = new OrderedDictionary<TKey, TValue>(capacity);

        foreach (var entry in this)
        {
            copy.Add((TKey)entry.Key, (TValue)entry.Value);
        }

        return copy;
    }

    // Overload del metodo Copy che implementa una deep copy se i valori sono ICloneable
    public OrderedDictionary<TKey, TValue> Copy(bool deepCopy)
    {
        if (!deepCopy)
            return Copy();

        var capacity = this.Count;
        var copy = new OrderedDictionary<TKey, TValue>(capacity);

        foreach (var entry in this)
        {
            TValue value = (TValue)entry.Value;
            TValue newValue;

            // Se il valore implementa ICloneable, usa il metodo Clone
            if (value is ICloneable cloneable)
            {
                newValue = (TValue)cloneable.Clone();
            }
            else
            {
                // Se non è cloneable, usa il valore originale (shallow copy)
                newValue = value;
            }

            copy.Add((TKey)entry.Key, newValue);
        }

        return copy;
    }

    public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var key in base.Keys)
        {
            yield return new KeyValuePair<TKey, TValue>(
                (TKey)key,
                (TValue)base[key]
            );
        }
    }

    // Implementazione esplicita di IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}