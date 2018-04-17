using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace LogAll
{
    public class TrackPerformanceFilter : IActionFilter
    {
        private PerformanceLogger _tracker;
        private string _application, _area;
        public TrackPerformanceFilter(string application, string area)
        {
            _application = application;
            _area = area;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var activity = $"{request.Path}-{request.Method}";

            var dict = new Dictionary<string, object>();
            foreach (var key in context.RouteData.Values?.Keys)
                dict.Add($"RouteData-{key}", (string)context.RouteData.Values[key]);

            var details = WebHelper.GetWebFlogDetail(_application, _area, activity,
                context.HttpContext, dict);

            _tracker = new PerformanceLogger(details);
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (_tracker != null)
                _tracker.Stop();
        }
    }
}
