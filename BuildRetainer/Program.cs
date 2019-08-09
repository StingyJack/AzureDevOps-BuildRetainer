using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildRetainer
{
    class Program
    {
        private static string _outputFileName = $"BuildRetainer_{DateTime.Now.ToFileTimeUtc()}.log";

        static void Main(string[] args)
        {
            var collectionUri = args[0];
            var pat = args[1];
            VssConnection connection = new VssConnection(new Uri(collectionUri), new VssBasicCredential(string.Empty, pat));

            var projectsClient = connection.GetClient<ProjectHttpClient>();

            var projects = projectsClient.GetProjects().Gar();
            var buildClient = connection.GetClient<BuildHttpClient>();

            foreach (var project in projects)
            {
                var builds = buildClient.GetBuildsAsync(project.Id).Gar();
                Info($"Project {project.Name} has {builds.Count} {nameof(builds)}");

                foreach (var build in builds)
                {
                    if (build.KeepForever == true)
                    {
                        Info($"The {nameof(build.KeepForever)} value for build {project.Name} - {build.Definition.Name} {build.BuildNumber} was already set to {true}");
                        continue;
                    }
                    var now = DateTime.Now;
                    build.KeepForever = true;
                    var updateResult = buildClient.UpdateBuildAsync(build, build.Id).Gar();
                    if (updateResult.LastChangedDate >= now)
                    {
                        Info($"Updated the {nameof(build.KeepForever)} value for build {project.Name} - {build.Definition.Name} {build.BuildNumber} to {true}");
                        continue;
                    }
                    Error($"Failed to set the {nameof(build.KeepForever)} value for build {project.Name} - {build.Definition.Name} {build.BuildNumber} to {true}");
                }
            }
        }

        private static void Info(string message)
        {
            var msg = $"{DateTime.UtcNow:hh:mm:ss} - info - {message}";
            System.IO.File.AppendAllLines(_outputFileName, new List<string> {msg  });
            Console.WriteLine(msg);
        }

        private static void Error(string message)
        {
            var msg = $"{DateTime.UtcNow:hh:mm:ss} - ERROR - {message}";
            System.IO.File.AppendAllLines(_outputFileName, new List<string> { msg });
            Console.Error.WriteLine(msg);
        }
    }

    internal static class Extensions
    {
        public static T Gar<T>(this Task<T> awaitable)
        {
            return awaitable.GetAwaiter().GetResult();
        }

    }
}
