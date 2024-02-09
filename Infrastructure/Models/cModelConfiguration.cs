namespace Solution.Infrastructure.Models;
public class cModelConfiguration
{
    public Dictionary<string,cModelConfigurationConnection> Connections { get; set; } = [];
    public Dictionary<string, cModelConfigurationQueue> Queues { get; set; } = [];
    public List<cModelConfigurationMessage> Messages { get; set; } = [];
    public string Target { get; set; } = string.Empty;
    public string InfrastructureConnection { get; set; } = string.Empty;
    public string PianifQueue { get; set; } = string.Empty;
    public string SystemQueue { get; set; } = string.Empty;
    public bool IsStandalone { get; set; } = false;
    public bool IsLocalAssembly { get; set; } = false;
    public string ExclusiveMessages { get; set; } = string.Empty;
}
public class cModelConfigurationConnection
{
    public cModelConfigurationConnection(string sKey, string sConnection, string sProvider)
    {
        Key = sKey;
        Connection = sConnection;
        Provider = sProvider;
    }
    public string Key { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
    public string Provider { get; set; } = "sqldb";
}

public class cModelConfigurationQueue
{
    public cModelConfigurationQueue(string sKey, string sConnection, string sTable)
    {
        Key = sKey;
        Connection = sConnection;
        Table = sTable;
    }
    public string Key { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;

}

public class cModelConfigurationMessage
{
    public cModelConfigurationMessage(string sAssembly, string sClass, string sFunction, string sValue)
    {
        Assembly = sAssembly;
        Class = sClass;
        Function = sFunction;
        Value = sValue;
    }
    public string Assembly { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}