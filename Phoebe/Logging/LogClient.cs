﻿using System;
using System.Collections.Generic;
using Toggl.Phoebe.Logging;
using Xamarin;

namespace Toggl.Phoebe.Logging
{
    public class LogClient : ILoggerClient
    {
        public LogClient (Action platformInitAction)
        {
            if (platformInitAction != null) {
                platformInitAction();
            }
        }

        #region ILoggerClient implementation

        public void SetUser (string id, string email = null, string name = null)
        {
            if (id != null) {
                var traits = new Dictionary<string, string> {
                    { Insights.Traits.Email, email },
                    { Insights.Traits.Name, name }
                };
                Insights.Identify (id, traits);
            }
        }

        public void Notify (Exception e, ErrorSeverity severity = ErrorSeverity.Error, Metadata extraMetadata = null)
        {
            var reportSeverity = Insights.Severity.Error;
            if (severity == ErrorSeverity.Warning) {
                reportSeverity = Insights.Severity.Warning;
            }

            var extraData = new Dictionary<string, string> ();
            foreach (var item in extraMetadata) {
                if (item.Value != null) {
                    var data = item.Value.ToObject<Dictionary<string, string>>();
                    foreach (var i in data) {
                        extraData.Add ( item.Key + ":" + i.Key, i.Value);
                    }
                }
            }

            if (severity == ErrorSeverity.Info) {
                Insights.Track ("Info", extraData);
            } else {
                Insights.Report (e, extraData, reportSeverity);
            }
        }

        public string DeviceId { get; set; }

        public List<string> ProjectNamespaces { get; set; }

        #endregion

    }
}

