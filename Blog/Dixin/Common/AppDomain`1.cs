namespace Dixin.Common
{
    using System;
    using System.Reflection;

    internal sealed class AppDomain<TMarshalByRefObject> : IDisposable
        where TMarshalByRefObject : MarshalByRefObject
    {
        private AppDomain appDomain;

        internal AppDomain(AppDomainSetup setup = null, params object[] args)
        {
            Type type = typeof(TMarshalByRefObject);
            this.appDomain = AppDomain.CreateDomain(
                $"{nameof(AppDomain<TMarshalByRefObject>)} {Guid.NewGuid()}",
                null,
                setup ?? AppDomain.CurrentDomain.SetupInformation);
            this.MarshalByRefObject = (TMarshalByRefObject)this.appDomain.CreateInstanceAndUnwrap(
                type.Assembly.FullName, type.FullName, false, default(BindingFlags), null, args, null, null);
        }

        internal TMarshalByRefObject MarshalByRefObject { get; private set; }

        public void Dispose()
        {
            IDisposable disposable = this.MarshalByRefObject as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
                this.MarshalByRefObject = null;
            }

            if (this.appDomain != null)
            {
                AppDomain.Unload(this.appDomain);
                this.appDomain = null;
            }
        }
    }
}