using Org.BouncyCastle.Crypto.Parameters;
using System.Reflection;

namespace Solution.Infrastructure;
public class FunctionsAssemblyManager
{
    private string AssemblyPath { get; set; } = string.Empty;
    FunctionAssemblyLoaderContext oAssemblies;
    public FunctionAssemblyLoaderContext Assemblies { get { return oAssemblies ??= new(AssemblyPath); } }
    readonly DB db = null;
    readonly string dbKey = null;
    public FunctionsAssemblyManager(DB oDB, string sKey, string sAssemblyPath)
    {
        db = oDB;
        dbKey = sKey;
        AssemblyPath = sAssemblyPath;
    }
    public Assembly LoadAssembly(string sAssemblyName)
    {
        return Assemblies.LoadAssembly(sAssemblyName);
    }
    public Assembly LoadAssembly(string sAssemblyName, byte[] assembly)
    {
        return Assemblies.LoadAssembly(sAssemblyName, assembly);
    }
    public void UnLoad()
    {
        oAssemblies?.Unload();
        oAssemblies = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
    public Type GetType(string sAssemblyName, string sClassName)
    {
        Assembly oAssembly = LoadAssembly(sAssemblyName);
        if (oAssembly is null)
        {
            throw new Exception($"Non è possibile caricare l'assembly: {sAssemblyName}");
        }
        return oAssembly.GetType(sClassName);
    }
    public object CallFunctionNoRemote(string sAssemblyName, string sClassName, string sMethodName, FunctionParameters oParameters)
    {
        Type? type = Type.GetType(sClassName + ", " + sAssemblyName.Replace(".dll", ""));
        MethodInfo? method = type.GetMethod(sMethodName);
        object? OBJ = Activator.CreateInstance(type);
        ((FunctionModule)OBJ).Load(db, dbKey, oParameters);
        return method.Invoke(OBJ, null);
    }
    public object CallFunction(string sAssemblyName, string sClassName, string sMethodName, FunctionParameters oParameters)
    {
        Type oType = GetType(sAssemblyName, sClassName);
        MethodInfo oMethod = oType.GetMethod(sMethodName);
        object oObject = CreateObject(oType, null);
        ((FunctionModule)oObject).Load(db, dbKey, oParameters);
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
    public object CreateObject(Type oType, object[] oParamsConstructor)
    {
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
}

public class FunctionAssemblyLoaderContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _assemblyDependencyResolver;
    private readonly HashSet<string> _defaultLoadedAssemblies = new HashSet<string>();
    private string AssemblyPath = "";

    public Assembly? this[string name] => Assemblies?.FirstOrDefault(c => c.GetName().Name.ToLower().Equals(name.ToLower().Replace(".dll", "")));

    public FunctionAssemblyLoaderContext(string pluginPath) : base(name: "FunctionAssemblyLoaderContext", isCollectible: true)
    {
        AssemblyPath = pluginPath;
        _assemblyDependencyResolver = new AssemblyDependencyResolver(pluginPath);
        //
        this.Unloading += PluginLoaderContext_Unloading;
        this.Resolving += PluginLoaderContext_Resolving;
        this.ResolvingUnmanagedDll += PluginLoaderContext_ResolvingUnmanagedDll;
        //foreach (var sharedAssembly in sharedAssemblies)
        //{
        //    AddToDefaultLoadedAssemblies(sharedAssembly);
        //}
    }
    private void PluginLoaderContext_Unloading(AssemblyLoadContext arg1)
    {
        Console.WriteLine($"PluginLoaderContext_Unloading: {arg1.Name}");
    }
    private Assembly PluginLoaderContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
    {
        Console.WriteLine($"PluginLoaderContext_Resolving: {arg1.Name}, {arg2.Name}");
        return null;
    }
    private nint PluginLoaderContext_ResolvingUnmanagedDll(Assembly arg1, string arg2)
    {
        Console.WriteLine($"PluginLoaderContext_ResolvingUnmanagedDll: {arg1.FullName}, {arg2}");
        return nint.Zero;
    }
    public Assembly LoadAssembly(string sAssemblyName)
    {
        Console.WriteLine($"Assembly Load: {sAssemblyName}");
        foreach (Assembly assembly in Assemblies)
        {
            if (assembly.GetName().Name.ToLower().Equals(sAssemblyName.ToLower().Replace(".dll", "")))
                return assembly;
        }
        return LoadFromStream(GetBytesByFile(Path.Combine(AssemblyPath, sAssemblyName)));
        //return LoadFromAssemblyPath(Path.Combine(AssemblyPath, sAssemblyName));
    }
    public Assembly LoadAssembly(string sAssemblyName, byte[] assembly)
    {
        return (this[sAssemblyName] ?? LoadFromStream(new MemoryStream(assembly)));
    }
    //private Assembly GetAssembly(string assemblyName)
    //{
    //    return Assemblies.FirstOrDefault(c => c.GetName().Name.ToLower().Equals(assemblyName.ToLower().Replace(".dll", "")));

    //    //foreach (Assembly assembly in Assemblies)
    //    //{
    //    //    if (assembly.GetName().Name.ToLower().Equals(sAssemblyName.ToLower().Replace(".dll", "")))
    //    //        return assembly;
    //    //}
    //    //return null;
    //}

    //public void AddToDefaultLoadedAssemblies(AssemblyName sharedAssembly)
    //{
    //    if (_defaultLoadedAssemblies.Contains(sharedAssembly.Name))
    //    {
    //        return;
    //    }

    //    _defaultLoadedAssemblies.Add(sharedAssembly.Name);

    //    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(sharedAssembly);
    //    foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
    //    {
    //        AddToDefaultLoadedAssemblies(referencedAssembly);
    //    }
    //}
    protected override Assembly Load(AssemblyName assemblyName)
    {
        Console.WriteLine($"override Assembly Load: {assemblyName.Name}");
        return null;
    }
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var assemblyPath = _assemblyDependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (assemblyPath != null)
        {
            return LoadUnmanagedDllFromPath(assemblyPath);
        }

        return base.LoadUnmanagedDll(unmanagedDllName);
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
}