namespace Dixin.Linq.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Text;

    internal static partial class Functions
    {
        internal static void Initialize(ProcessStartInfo processStart)
        {
            processStart.FileName = "File";
            processStart.Arguments = "Arguments";
            processStart.UseShellExecute = false;
            processStart.Environment["Variable"] = "value";
            processStart.UserName = "UserName";
            processStart.PasswordInClearText = "Password";
            const object a = null;
        }
    }
}
