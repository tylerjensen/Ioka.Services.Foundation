using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog.Extensions.Logging;
using System;
using System.Reflection;

namespace Ioka.Services.Foundation.Logging
{
    public static class LogContextFactory
    {
        private static Parser _uaParser = Parser.GetDefault();

        public static LogContext Create(HttpContext context)
        {
            return CreateFactory(() => context)();
        }

        public static Func<LogContext> CreateFactory(ContextProvider contextProvider)
        {
            var httpContextFactory = new Func<HttpContext>(() => contextProvider.GetCurrent());
            return CreateFactory(httpContextFactory);
        }

        public static Func<LogContext> CreateFactory(Func<HttpContext> httpContextFactory)
        {
            if (null == httpContextFactory) throw new ArgumentNullException(nameof(httpContextFactory));
            return new Func<LogContext>(() =>
            {
                try
                {
                    var httpCtx = httpContextFactory();
                    var httpRequestFeature = httpCtx.Request.HttpContext.Features.Get<IHttpRequestFeature>();
                    var context = new LogContext();
                    context["_ThreadId"] = Environment.CurrentManagedThreadId.ToString(); //set to current thread
                    context["_Source"] = Assembly.GetEntryAssembly().GetName().Name;
                    context["_IpAddress"] = httpCtx.Connection.RemoteIpAddress.ToString();
                    context["_UserId"] = httpCtx.Request.Headers["APPUID"].Count > 0 ? httpCtx.Request.Headers["APPUID"][0] : context["_UserId"];
                    context["_HttpMethod"] = httpCtx.Request.Method;

                    //allow reuse of requestId in logger enricher
                    if (null == httpCtx.Items["_RequestId"])
                    {
                        httpCtx.Items["_RequestId"] = httpCtx.Request.Headers["APPREQID"].Count > 0 
                            ? httpCtx.Request.Headers["APPREQID"][0] 
                            : Guid.NewGuid().ToString();
                    }
                    context["_RequestId"] = httpCtx.Items["_RequestId"].ToString();
                    context["_Url"] = httpCtx.Request.Scheme + "://" + httpCtx.Request.Host + httpCtx.Request.Path;
                    context["_Query"] = httpCtx.Request.Query.ToString();
                    context["_WebUser"] = (null != httpCtx.User && null != httpCtx.User.Identity) ? httpCtx.User.Identity.Name : "anonymous";

                    //browser
                    try
                    {
                        StringValues uaVal;
                        if (httpCtx.Request.Headers.TryGetValue("User-Agent", out uaVal))
                        {
                            var ua = uaVal.ToString();
                            ClientInfo bc = _uaParser.Parse(ua);
                            context["_Browser"] = $"{bc.UA.Family} {bc.UA.Major}.{bc.UA.Minor}.{bc.UA.Patch} {bc.OS.Family} {bc.OS.Major}";
                        }
                    }
                    catch (Exception e)
                    {
                        context[$"_Browser_err"] = $"{e}";
                    }

                    //cookies
                    try
                    {
                        foreach (var item in httpCtx.Request.Cookies)
                        {
                            context[$"_Cookie_{item.Key}"] = $"{item.Value}";
                        }
                    }
                    catch (Exception e)
                    {
                        context[$"_Cookie_err"] = $"{e}";
                    }

                    //form -- remove anything with key containing password
                    try
                    {
                        if (httpCtx.Request.Method == "POST" && null != httpCtx.Request.Form && httpCtx.Request.Form.Count > 0)
                        {
                            foreach (var item in httpCtx.Request.Form)
                            {
                                var val = item.Value;
                                if (item.Key.ToLower().Contains("password"))
                                {
                                    val = "***"; //mask pwd in ELK
                                }
                                context[$"_Form_{item.Key}"] = $"{val}";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        context[$"_Form_err"] = $"{e}";
                    }

                    //headers
                    try
                    {
                        if (null != httpCtx.Request.Headers && httpCtx.Request.Headers.Count > 0)
                        {
                            foreach (var item in httpCtx.Request.Headers)
                            {
                                context[$"_Header_{item.Key}"] = $"{item.Value}";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        context[$"_Header_err"] = $"{e}";
                    }
                    return context;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            });
        }
    }
}
