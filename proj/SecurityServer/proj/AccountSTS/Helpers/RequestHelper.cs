﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using Dragon.SecurityServer.AccountSTS.Controllers;
using Dragon.SecurityServer.Common;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;

namespace Dragon.SecurityServer.AccountSTS.Helpers
{
    public static class RequestHelper
    {
        public static string GetCurrentServiceId()
        {
            return GetParameterFromReturnUrl(Consts.QueryStringParameterNameServiceId);
        }

        public static string GetParameterFromReturnUrl(string parameter)
        {
            var returnUrl = new Uri(
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) +
                HttpContext.Current.GetOwinContext().Request.Query[Consts.QueryStringParameterNameReturnUrl]);
            var currentServiceId = HttpUtility.ParseQueryString(returnUrl.Query).Get(parameter);
            if (string.IsNullOrEmpty(currentServiceId)) throw new InvalidParameterException();
            return currentServiceId;
        }

        public static RouteValueDictionary ReturnUrlToRouteValues(NameValueCollection source, object additionalProperties)
        {
            var result = ReturnUrlToRouteValues(source);
            foreach (var property in additionalProperties.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                result.Add(property.Name, property.GetValue(additionalProperties, null));
            }
            return result;
        }

        public static RouteValueDictionary ReturnUrlToRouteValues(NameValueCollection source)
        {
            return ToRouteValueDictionary(source, new List<string> {Consts.QueryStringParameterNameReturnUrl});
        }

        public static RouteValueDictionary ToRouteValues(NameValueCollection source)
        {
            return ToRouteValueDictionary(source, new List<string>());
        }

        private static RouteValueDictionary ToRouteValueDictionary(NameValueCollection source, ICollection<string> includeOnly)
        {
            var routeValues = new RouteValueDictionary();
            if (source == null)
            {
                return routeValues;
            }
            foreach (string key in source.Keys)
            {
                if (null != includeOnly && includeOnly.Any() && !includeOnly.Contains(key))
                {
                    continue;
                }
                routeValues.Add(key, source[key]);
            }
            return routeValues;
        }
    }
}