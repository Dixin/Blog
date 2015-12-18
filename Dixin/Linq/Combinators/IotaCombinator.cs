namespace Dixin.Linq.Combinators
{
    using System;

    public static class IotaCombinator
    {
        public static Func<dynamic, dynamic>
            ι = f => f
                (new Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>(x => y => z => x(z)(y(z)))) // S
                (new Func<dynamic, Func<dynamic, dynamic>>(x => y => x)); // K

        public static Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>
            S = ι(ι(ι(ι(ι))));

        public static Func<dynamic, Func<dynamic, dynamic>>
            K = ι(ι(ι(ι)));

        public static Func<dynamic, dynamic>
            I = ι(ι);
    }
}
