namespace Examples.Common
{
    using System;

    internal sealed class AppDomain<TMarshalByRefObject> : IDisposable
        where TMarshalByRefObject : MarshalByRefObject
    {
        private AppDomain? appDomain;

        internal AppDomain(params object[] args)
        {
            Type type = typeof(TMarshalByRefObject);
            this.appDomain = AppDomain.CreateDomain($"{nameof(AppDomain<TMarshalByRefObject>)} {Guid.NewGuid()}");
            this.MarshalByRefObject = (TMarshalByRefObject?)this.appDomain.CreateInstanceAndUnwrap(
                type.Assembly.FullName ?? throw new ArgumentException(nameof(TMarshalByRefObject)), 
                type.FullName ?? throw new ArgumentException(nameof(TMarshalByRefObject)), 
                false, 
                default, 
                null, 
                args, 
                null, 
                null);
        }

        internal TMarshalByRefObject? MarshalByRefObject { get; private set; }

        public void Dispose()
        {
            if (this.MarshalByRefObject is IDisposable disposable)
            {
                disposable?.Dispose();
                this.MarshalByRefObject = null;
            }

            if (this.appDomain is not null)
            {
                AppDomain.Unload(this.appDomain);
                this.appDomain = null;
            }
        }
    }
}