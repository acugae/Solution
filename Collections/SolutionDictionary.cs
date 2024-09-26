using System;
using System.Collections;
using System.Collections.Generic;

public class SolutionDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IComparable<TKey>
{
    private List<TKey> _keysInsertionOrder = new List<TKey>(); // Per l'ordine di inserimento
    private SortedDictionary<TKey, TValue> _sortedDictionary = new SortedDictionary<TKey, TValue>(); // Per l'ordine delle chiavi

    public void Add(TKey key, TValue value)
    {
        if (!_sortedDictionary.ContainsKey(key))
        {
            _keysInsertionOrder.Add(key);
            _sortedDictionary.Add(key, value);
        }
        else
        {
            throw new ArgumentException("Chiave già presente nel dizionario.");
        }
    }

    public bool Remove(TKey key)
    {
        if (_sortedDictionary.Remove(key))
        {
            _keysInsertionOrder.Remove(key);
            return true;
        }
        return false;
    }

    public TValue this[TKey key]
    {
        get => _sortedDictionary[key];
        set
        {
            if (_sortedDictionary.ContainsKey(key))
            {
                _sortedDictionary[key] = value;
            }
            else
            {
                _keysInsertionOrder.Add(key);
                _sortedDictionary.Add(key, value);
            }
        }
    }

    // Iterazione per ordine di inserimento
    public IEnumerable<KeyValuePair<TKey, TValue>> ByInsertionOrder()
    {
        foreach (var key in _keysInsertionOrder)
        {
            yield return new KeyValuePair<TKey, TValue>(key, _sortedDictionary[key]);
        }
    }

    // Iterazione per ordine di chiavi
    public IEnumerable<KeyValuePair<TKey, TValue>> ByKeyOrder()
    {
        foreach (var kvp in _sortedDictionary)
        {
            yield return kvp;
        }
    }

    // Implementazione dell'interfaccia IEnumerable per default (potresti scegliere uno di questi due metodi come default)
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return ByInsertionOrder().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}