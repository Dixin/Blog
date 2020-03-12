namespace Examples.Data.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Threading.Tasks;
    using System.Web.Routing;

    using Microsoft.ServiceModel.WebSockets;

    public interface IWebSocketService
    {
        Task Send(string value);
    }

    public interface IWebSocketConnections : IEnumerable<IWebSocketService>
    {
        Task[] Send(string value);
    }

    public class WcfWebSocketService : WebSocketService, IWebSocketService
    {
        public override void OnOpen() => WebSocketConnections.All.Add(this);

        protected override void OnClose() => WebSocketConnections.All.Remove(this);

        public static void Register(RouteCollection routes, string name = "NotificationService") =>
            routes.Add(new ServiceRoute(name, new WebSocketServiceHostFactory(), typeof(WcfWebSocketService)));
    }

    public class WebSocketConnections : IWebSocketConnections
    {
        private readonly WebSocketCollection<WcfWebSocketService> connections = new WebSocketCollection<WcfWebSocketService>();

        private WebSocketConnections()
        {
        }

        public static WebSocketConnections All { get; } = new WebSocketConnections();

        public IEnumerator<IWebSocketService> GetEnumerator() => this.connections.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public Task[] Send(string value) =>
            this.connections.Select(notificationService => notificationService.Send(value)).ToArray();

        public void Add(WcfWebSocketService connection) => this.connections.Add(connection);

        public void Remove(WcfWebSocketService connection) => this.connections.Remove(connection);
    }

    public class WebSocketServiceHostFactory : ServiceHostFactory
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            WebSocketHost host = new WebSocketHost(serviceType, baseAddresses);
            host.AddWebSocketEndpoint();
            return host;
        }
    }
}
