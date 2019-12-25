using System;
using System.Linq;
using System.Collections.Generic;
using Harmonic.Controllers.Living;

namespace Harmonic.Service
{
    public class PublisherSessionService
    {
        private readonly Dictionary<string, LivingStream> _pathMapToSession = new Dictionary<string, LivingStream>();
        private readonly Dictionary<LivingStream, string> _sessionMapToPath = new Dictionary<LivingStream, string>();

        //这里的publishingName就是/living/touhou中/touhou和
        internal void RegisterPublisher(string publishingName, LivingStream session)
        {
            var oldPublishingName = publishingName;
            var pub = publishingName.Split('?', StringSplitOptions.RemoveEmptyEntries);
            publishingName = pub[0];
            if(pub.Length > 1)
            {
                var pubParam = pub[1].Split('&');
                var params1 = pubParam.Select(x => x.Split('=',StringSplitOptions.None)).Select(x => new KeyValuePair<string, string>(x[0], x[1]));

                if (params1.SingleOrDefault(x => x.Key == "uname").Value == "coredx" && params1.SingleOrDefault(x => x.Key == "pass").Value == "123456")
                {

                }
                else
                {
                    throw new InvalidOperationException("账号信息错误");
                }
            }

            if (_pathMapToSession.ContainsKey(publishingName))
            {
                throw new InvalidOperationException(Resource.request_instance_is_publishing);
            }
            if (_sessionMapToPath.ContainsKey(session))
            {
                throw new InvalidOperationException(Resource.request_session_is_publishing);
            }
            _pathMapToSession.Add(publishingName, session);
            _sessionMapToPath.Add(session, publishingName);
        }

        internal void RemovePublisher(LivingStream session)
        {
            if (_sessionMapToPath.TryGetValue(session, out var publishingName))
            {
                _sessionMapToPath.Remove(session);
                _pathMapToSession.Remove(publishingName);
            }
        }
        public LivingStream FindPublisher(string publishingName)
        {
            if (_pathMapToSession.TryGetValue(publishingName, out var session))
            {
                return session;
            }
            return null;
        }

    }
}