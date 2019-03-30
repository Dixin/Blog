namespace Tutorial.Properties

    open System
    open System.Reflection
    open System.Resources
    open System.Runtime.InteropServices

    [<assembly: AssemblyDescription("Code examples for https://weblogs.asp.net/dixin/linq-via-csharp.")>]
    [<assembly: AssemblyCopyright("Dixin Yan https://weblogs.asp.net/dixin")>]

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<assembly: AssemblyFileVersion("1.0.0.0")>]
    [<assembly: AssemblyCompany("Dixin Yan")>]
    [<assembly: AssemblyProduct("Tutorial")>]
#endif

    [<assembly: CLSCompliant(false)>]
    [<assembly: NeutralResourcesLanguage("en-US")>]

#if NETFX || WINDOWS_UWP
    [<assembly: ComVisible(false)>]
#endif

    do ()