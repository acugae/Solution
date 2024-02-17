using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace Solution.Infrastructure;
public class FunctionsAssemblyContext : AssemblyLoadContext, IDisposable
{
    private string AssemblyPath { get; set; } = string.Empty;
    Dictionary<string, Assembly> ocAssemblies = [];
    private AssemblyDependencyResolver _resolver;
    readonly DB DB = null;
    public FunctionsAssemblyContext(DB oDB, string sAssemblyPath) : base(name: "FunctionsAssemblyContext", isCollectible: true)
    {
        DB = oDB;
        AssemblyPath = sAssemblyPath;
        this.Resolving += CFunctionsAssemblyContext_Resolving;
        this.Unloading += CFunctionsAssemblyContext_Unloading;
        //_resolver = new(sAssemblyPath);
    }

    private void CFunctionsAssemblyContext_Unloading(AssemblyLoadContext obj)
    {
        Console.WriteLine($"Unloading");
    }

    private Assembly CFunctionsAssemblyContext_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        Console.WriteLine($"Coundn't resovle {assemblyName.Name}");
        return null; // Fail to read.
    }

    protected override Assembly Load(AssemblyName assemblyName) { return null; }
    //protected override Assembly Load(AssemblyName assemblyName)
    //{
    //    if (_resolver != null)
    //    {
    //        //if (!ocAssemblies.ContainsKey(assemblyName.Name))
    //        //    LoadAssembly(assemblyName.Name, GetBytesByFile(Path.Combine(AssemblyPath, assemblyName.Name)));            
    //        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
    //        if (assemblyPath != null)
    //        {
    //            return LoadFromAssemblyPath(Path.Combine(AssemblyPath, assemblyName.Name));
    //        }
    //    }
    //    return null;
    //}
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
    public void LoadAssembly(string sAssemblyName, Stream sAssembly, bool bForceLoad = false)
    {
        if (!ocAssemblies.ContainsKey(sAssemblyName) || bForceLoad)
        {
            Assembly oAssembly = LoadFromStream(sAssembly);
            ocAssemblies[sAssemblyName] = oAssembly;
        }
    }
    public Assembly this[string sAssemblyName]
    {
        get
        {
            if (!ocAssemblies.ContainsKey(sAssemblyName))
                throw new Exception("Assembly not present.");
            return ocAssemblies[sAssemblyName];
        }
    }
    private MemoryStream GetBytesByFile(string sFile)
    {
        byte[] oFileBytes = null;
        using (FileStream fs = File.Open(sFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            int numBytesToRead = Convert.ToInt32(fs.Length);
            oFileBytes = new byte[numBytesToRead];
            fs.Read(oFileBytes, 0, numBytesToRead);
            fs.Close();
        }
        return new MemoryStream(oFileBytes);
    }
    public object CallFunction(string sAssemblyName, string sClassName, string sMethodName, FunctionParameters oParameters)
    {
        if (!ocAssemblies.ContainsKey(sAssemblyName))
            LoadAssembly(sAssemblyName, GetBytesByFile(Path.Combine(AssemblyPath, sAssemblyName)));
        //LoadAssembly(sAssemblyName, DB.GetBytesByDB(sAssemblyName));
        //
        MethodInfo oMethod = ocAssemblies[sAssemblyName].GetType(sClassName).GetMethod(sMethodName);
        object oObject = CreateObject(sAssemblyName, sClassName, null);
        ((Module)oObject).Load(DB, oParameters);
        //
        ParameterInfo[] ovPI = oMethod.GetParameters();
        List<object> oParams = null;
        if (ovPI != null)
        {
            oParams = new List<object>();
            for (int i = 0; i < ovPI.Length; i++)
            {
                if (oParameters.ContainsKey(ovPI[i].Name) && oParameters[ovPI[i].Name] != null)
                {
                    if (oParameters[ovPI[i].Name].GetType().Equals(ovPI[i].ParameterType))
                        oParams.Add(oParameters[ovPI[i].Name]);
                    else
                    {
                        if (ovPI[i].Name.Equals("Value"))
                        {
                            if (string.IsNullOrEmpty(oParameters["Value"].ToString()))
                                oParams.Add(null);
                            else
                            {
                                JToken ojObject = JToken.Parse(oParameters["Value"].ToString());
                                JsonSerializer serializer = new JsonSerializer();
                                oParams.Add(serializer.Deserialize(new JTokenReader(ojObject), ovPI[i].ParameterType));
                            }
                        }
                        else
                            oParams.Add(Convert.ChangeType(oParameters[ovPI[i].Name], ovPI[i].ParameterType));
                    }
                }
                else
                {
                    if (ovPI[i].HasDefaultValue)
                        oParams.Add(ovPI[i].DefaultValue);
                }
            }
        }
        object oResult = CallMethod(oObject, oMethod, oParams.ToArray());
        return oResult;
    }
    public object CreateObject(string sAssemblyName, string strClassName, object[] oParamsConstructor)
    {
        Type oType = ocAssemblies[sAssemblyName].GetType(strClassName);
        if (oParamsConstructor == null)
            return Activator.CreateInstance(oType);
        return Activator.CreateInstance(oType, oParamsConstructor.ToArray());
    }
    public object CallMethod(object oObject, MethodInfo oMethod, object[] oParams)
    {
        try
        {
            if (oParams != null && oParams.Length > 0)
                return oMethod.Invoke(oObject, oParams);
            else
                return oMethod.Invoke(oObject, null);
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    //public new void Unload()
    //{
    //    ocAssemblies.Clear(); // Without this call, the assemblies in this dictionary don't get unloaded.
    //    base.Unload();
    //}

    public void Dispose()
    {
        ocAssemblies.Clear(); // Without this call, the assemblies in this dictionary don't get unloaded.
        Unload();
    }
}
