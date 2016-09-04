namespace Dixin.Linq.CSharp
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static partial class Functions
    {
        internal static bool IsPositive(int int32)
        {
            return int32 > 0;
        }

        internal static void NamedFunction()
        {
            Func<int, bool> isPositive = IsPositive;
            bool result = isPositive(0);
        }
    }

    internal static partial class Functions
    {
        internal static void AnonymousFunction()
        {
            Func<int, bool> isPositive = delegate (int int32)
            {
                return int32 > 0;
            };
            bool result = isPositive(0);
        }
    }

    internal static partial class CompiledFunctions
    {
        [CompilerGenerated]
        private static Func<int, bool> cachedIsPositive;

        [CompilerGenerated]
        private static bool IsPositive(int int32)
        {
            return int32 > 0;
        }

        internal static void AnonymousFunction()
        {
            Func<int, bool> isPositive;
            if (cachedIsPositive != null)
            {
                isPositive = cachedIsPositive;
            }
            else
            {
                isPositive = cachedIsPositive = new Func<int, bool>(IsPositive);
            }
            bool result = isPositive.Invoke(0);
        }
    }

    internal static partial class Functions
    {
        internal static void Lambda()
        {
            Func<int, bool> isPositive = (int int32) =>
                {
                    return int32 > 0;
                };
            bool result = isPositive(0);
        }

        internal static void ExpressionLambda()
        {
            Func<int, int, int> add = (int32A, int32B) => int32A + int32B;
            Func<int, bool> isPositive = int32 => int32 > 0;
            Action<int> traceLine = int32 => Trace.WriteLine(int32);
        }

        internal static void StatementLambda()
        {
            Func<int, int, int> add = (int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            };
            Func<int, bool> isPositive = int32 =>
            {
                Trace.WriteLine(int32);
                return int32 > 0;
            };
            Action<int> traceLine = int32 =>
            {
                Trace.WriteLine(int32);
                Trace.Flush();
            };
        }

        internal static void ConstructorCall()
        {
            Func<int, int, int> add = new Func<int, int, int>((int32A, int32B) => int32A + int32B);

            Func<int, bool> isPositive = new Func<int, bool>(int32 =>
            {
                Trace.WriteLine(int32);
                return int32 > 0;
            });
        }

#if DEMO
        internal static void CallLambdaExpression()
        {
            (int32 => int32 > 0)(1); // Define an expression lambda and call.
        }
#endif
    }

    internal static partial class Functions
    {
        internal static void CallLambdaExpression()
        {
            new Func<int, bool>(int32 => int32 > 0)(1);
        }
    }

    internal static partial class CompiledFunctions
	{
		[CompilerGenerated]
		[Serializable]
		private sealed class Container
		{
			public static readonly Container Singleton = new Container();

			public static Func<int, bool> cachedIsPositive;

			internal bool IsPositive(int int32)
			{
				return int32 > 0;
			}
		}

		internal static void CallLambdaExpression()
		{
            Func<int, bool> isPositive;
		    if (Container.cachedIsPositive != null)
		    {
		        isPositive = Container.cachedIsPositive;
		    }
		    else
		    {
                isPositive = Container.cachedIsPositive = new Func<int, bool>(Container.Singleton.IsPositive);
            }
			isPositive.Invoke(1);
		}
	}

    internal static partial class Functions
    {
        internal static void CallAnonymousFunction()
        {
            new Func<int, int, int>((int32A, int32B) => int32A + int32B)(1, 2);
            new Action<int>(int32 => Trace.WriteLine(int32))(1);

            new Func<int, int, int>((int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            })(1, 2);
            new Func<int, bool>(int32 =>
            {
                Trace.WriteLine(int32);
                return int32 > 0;
            })(1);
            new Action<int>(int32 =>
            {
                Trace.WriteLine(int32);
                Trace.Flush();
            })(1);
        }
    }
}
