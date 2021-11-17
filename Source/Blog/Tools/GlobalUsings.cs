global using System.Diagnostics.CodeAnalysis;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Globalization;
global using System.Linq.Expressions;
global using System.Net;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Text;
#if NET
global using System.Text.Json;
#endif
global using System.Text.RegularExpressions;
global using System.Xml.Linq;

// For .NET Framework. NETFRAMEWORK or NET48 does not work. Must use !NET.
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives
#if !NET
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;
#endif