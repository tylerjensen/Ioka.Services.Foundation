using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Ioka.Services.Foundation.Logging
{
    public class LogEnricher : ILogEventEnricher
    {
        private readonly LogContext _globalContext;
        private readonly Func<LogContext> _runtimeContextCreator;

        public LogEnricher() : this(null, null)
        {
        }

        public LogEnricher(LogContext globalContext, Func<LogContext> runtimeContextCreator)
        {
            _globalContext = globalContext ?? new LogContext();
            _runtimeContextCreator = runtimeContextCreator;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (null != _runtimeContextCreator)
            {
                var runtimeContext = _runtimeContextCreator();
                if (null != runtimeContext)
                {
                    foreach (var item in runtimeContext)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Value))
                        {
                            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(item.Key, item.Value));
                        }
                    }
                }
            }
            foreach (var item in _globalContext)
            {
                // Adds global only if not first written by runtime context
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(item.Key, item.Value));
            }
        }
    }


    /// <summary>
    /// Holds properties that can be attached to log events to be used in LogEnricher.
    /// </summary>
    public class LogContext : IDictionary<string, string>
    {
        public LogContext()
        {
            this.props = new Dictionary<string, string>();
#if NETSTANDARD2_0
            var userId = Environment.UserName;
#else
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            var userId = currentUser.Name;
#endif
            var hostName = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostName).Where(ip => !IPAddress.IsLoopback(ip)
                && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToArray();
            this["_UserId"] = userId;
            this["_IpAddress"] = addresses.Length > 0 ? addresses[0].ToString() : null;
            this["_Source"] = Assembly.GetEntryAssembly()?.GetName().Name; //in web as class lib, GetEntryAssembly returns null and factory will have to supply it
            this["_MachineName"] = Environment.MachineName;
            this["_ThreadId"] = Environment.CurrentManagedThreadId.ToString();
        }

        private Dictionary<string, string> props { get; set; }

        public string this[string key]
        {
            get => props[key];
            set
            {
                props[key] = value;
            }
        }

        public ICollection<string> Keys => props.Keys;

        public ICollection<string> Values => props.Values;

        public int Count => props.Count;

        public bool IsReadOnly => false;

        public void Add(string key, string value)
        {
            props.Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            props.Clear();
            //Serilog.Context.LogContext.Reset();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return props.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return props.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((IDictionary<string, string>)props).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return props.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return props.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return props.Remove(item.Key);
        }

        public bool TryGetValue(string key, out string value)
        {
            string val;
            var retval = props.TryGetValue(key, out val);
            value = val;
            return retval;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator() as IEnumerator;
        }
    }
}
