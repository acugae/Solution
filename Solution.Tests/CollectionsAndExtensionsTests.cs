using System.Data;
using Solution.Collections;

namespace Solution.Tests;

public class CollectionsAndExtensionsTests
{
    [Fact]
    public void GCollection_PreservesOrderAndRaisesEvents()
    {
        var collection = new GCollection<string, int>();
        object? addedKey = null;
        object? removedKey = null;
        object? removedIndex = null;

        collection.OnAdd += values => addedKey = values[1];
        collection.OnRemove += values => removedKey = values[1];
        collection.OnRemoveFromIndex += values => removedIndex = values[1];

        collection.Add("first", 1);
        collection.Insert("second", 2, 0);
        collection.Set("first", 10);
        collection.Remove("first");
        collection.RemoveFromIndex(0);

        Assert.Equal("second", addedKey);
        Assert.Equal("first", removedKey);
        Assert.Equal(0, removedIndex);
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void GCollection_Swap_ExchangesElementsWithoutChangingValues()
    {
        var collection = new GCollection<string, int>();
        collection.Add("first", 1);
        collection.Add("second", 2);

        collection.Swap(0, 1);

        Assert.Equal("second", collection.GetKey(0));
        Assert.Equal("first", collection.GetKey(1));
        Assert.Equal(2, collection.GetValue(0));
        Assert.Equal(1, collection.GetValue(1));
    }

    [Fact]
    public void DataRowExtensions_ConvertDbNullToNullAndMapToObject()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        table.Columns.Add("OptionalAge", typeof(int));
        var row = table.Rows.Add("Ada", 32, DBNull.Value);

        var dictionary = row.ToDictionary();
        var mapped = row.To<Person>();

        Assert.Equal("Ada", dictionary["Name"]);
        Assert.Null(dictionary["OptionalAge"]);
        Assert.Equal("Ada", mapped.Name);
        Assert.Equal(32, mapped.Age);
        Assert.Null(mapped.OptionalAge);
    }

    private sealed class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public int? OptionalAge { get; set; }
    }
}
