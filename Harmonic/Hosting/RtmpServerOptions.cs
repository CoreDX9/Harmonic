using Harmonic.Controllers;
using Harmonic.Networking.Rtmp;
using Harmonic.Networking.Rtmp.Data;
using Harmonic.Networking.Rtmp.Messages;
using Harmonic.Networking.Rtmp.Messages.Commands;
using Harmonic.Networking.Rtmp.Messages.UserControlMessages;
using Harmonic.Networking.Rtmp.Serialization;
using Harmonic.Rpc;
using Harmonic.Service;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace Harmonic.Hosting
{
    public class RtmpServerOptions
    {
        internal Dictionary<MessageType, MessageFactory> _messageFactories = new Dictionary<MessageType, MessageFactory>();
        public IReadOnlyDictionary<MessageType, MessageFactory> MessageFactories => _messageFactories;
        public delegate Message MessageFactory(MessageHeader header, SerializationContext context, out int consumed);
        private Dictionary<string, Type> _registeredControllers = new Dictionary<string, Type>();
        internal IServiceCollection _services = null;
        private RpcService _rpcService = null;
        internal IStartup _startup = null;
        internal IStartup Startup
        {
            get
            {
                return _startup;
            }
            set
            {
                _startup = value;
                _services = new ServiceCollection();
                _startup.ConfigureServices(_services);
                RegisterCommonServices(_services);
            }
        }
        internal IServiceProvider ApplicationServiceProvider { get; private set; }
        internal IServiceProvider ScopedServiceProvider { get; private set; }

        internal IReadOnlyDictionary<string, Type> RegisteredControllers => _registeredControllers;
        internal IPEndPoint RtmpEndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 1935);
        internal bool UseUdp { get; set; } = true;
        internal bool UseWebsocket { get; set; } = true;
        internal X509Certificate2 Cert { get; set; }

        internal RtmpServerOptions()
        {
            var userControlMessageFactory = new UserControlMessageFactory();
            var commandMessageFactory = new CommandMessageFactory();
            RegisterMessage<AbortMessage>();
            RegisterMessage<AcknowledgementMessage>();
            RegisterMessage<SetChunkSizeMessage>();
            RegisterMessage<SetPeerBandwidthMessage>();
            RegisterMessage<WindowAcknowledgementSizeMessage>();
            RegisterMessage<DataMessage>((MessageHeader header, SerializationContext context, out int consumed) =>
            {
                consumed = 0;
                return new DataMessage(header.MessageType == MessageType.Amf0Data ? AmfEncodingVersion.Amf0 : AmfEncodingVersion.Amf3);
            });
            RegisterMessage<VideoMessage>();
            RegisterMessage<AudioMessage>();
            RegisterMessage<UserControlMessage>(userControlMessageFactory.Provide);
            RegisterMessage<CommandMessage>(commandMessageFactory.Provide);
            _rpcService = new RpcService();
        }

        internal void BuildContainer()
        {
            ApplicationServiceProvider = _services.BuildServiceProvider();
            ScopedServiceProvider = ApplicationServiceProvider.CreateScope().ServiceProvider;
        }

        public void RegisterMessage<T>(MessageFactory factory) where T : Message
        {
            var tType = typeof(T);
            var attr = tType.GetCustomAttribute<RtmpMessageAttribute>();
            if (attr == null)
            {
                throw new InvalidOperationException();
            }

            foreach (var messageType in attr.MessageTypes)
            {
                _messageFactories.Add(messageType, factory);
            }
        }

        public void RegisterMessage<T>() where T : Message, new()
        {
            var tType = typeof(T);
            var attr = tType.GetCustomAttribute<RtmpMessageAttribute>();
            if (attr == null)
            {
                throw new InvalidOperationException();
            }

            foreach (var messageType in attr.MessageTypes)
            {
                _messageFactories.Add(messageType, (MessageHeader a, SerializationContext b, out int c) =>
                {
                    c = 0;
                    return new T();
                });
            }
        }

        internal void RegisterController(Type controllerType, string appName = null)
        {
            if (!typeof(RtmpController).IsAssignableFrom(controllerType))
            {
                throw new InvalidOperationException("controllerType must inherit from AbstractController");
            }
            var name = appName ?? controllerType.Name.Replace("Controller", "");
            _registeredControllers.Add(name.ToLower(), controllerType);
            _rpcService.RegeisterController(controllerType);
            _services.AddTransient(controllerType);
        }
        internal void RegisterStream(Type streamType)
        {
            if (!typeof(NetStream).IsAssignableFrom(streamType))
            {
                throw new InvalidOperationException("streamType must inherit from NetStream");
            }
            _rpcService.RegeisterController(streamType);
            _services.AddTransient(streamType);
        }

        internal void CleanupRpcRegistration()
        {
            _rpcService.CleanupRegistration();
        }
        private void RegisterCommonServices(IServiceCollection services)
        {
            services.AddTransient<RecordServiceConfiguration>();
            services.AddScoped<RecordService>();
            services.AddScoped<PublisherSessionService>();
            services.AddSingleton(_rpcService);
        }

        internal void RegisterController<T>(string appName = null) where T : RtmpController
        {
            RegisterController(typeof(T), appName);
        }
        internal void RegisterStream<T>() where T : NetStream
        {
            RegisterStream(typeof(T));
        }
    }
}
