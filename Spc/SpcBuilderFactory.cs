using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib;
using System;
using FileEmulationFramework.Lib.Utilities;
namespace SPC.Stream.Emulator.Spc
{
    internal class SpcBuilderFactory
    {
        private List<RouteGroupTuple> _routeGroupTuples = new();

        /// <summary>
        /// Adds all available routes from folders.
        /// </summary>
        /// <param name="redirectorFolder">Folder containing the redirector's files.</param>
        public void AddFromFolders(string redirectorFolder, Logger log = null)
        {
            redirectorFolder = redirectorFolder.Replace("/", "\\");
            // Get contents.
            WindowsDirectorySearcher.GetDirectoryContentsRecursiveGrouped(redirectorFolder, out var groups);

            // Find matching folders.
            foreach (var group in groups)
            {
                if (group.Files.Length <= 0)
                    continue;
                log?.Debug("Adding " + "\n  " + redirectorFolder + "\n  " + group.Directory.FullPath + "\n     " + group.Files[0]);
                var route = Route.GetRoute(redirectorFolder, group.Directory.FullPath);
                log?.Debug(route);
                /*try
                {
                        route = route.Substring(0, route.IndexOf("\\"));
                }
                catch (Exception e)
                {
                    log?.Error(e.ToString());
                    return;
                }*/
                log?.Debug("Route: " + route);
                _routeGroupTuples.Add(new RouteGroupTuple()
                {
                    Route = new Route(route),
                    Files = group
                });
            }
        }

        /// <summary>
        /// Tries to create an WAD from a given route.
        /// </summary>
        /// <param name="path">The file path/route to create WAD Builder for.</param>
        /// <param name="builder">The created builder.</param>
        /// <returns>True if a builder could be made, else false (if there are no files to modify this WAD).</returns>
        public bool TryCreateFromPath(string path, Logger log, out SpcBuilder? builder)
        {
            builder = default;
            var route = new Route(path);
            log?.Debug("Path: " + path);

            foreach (var group in _routeGroupTuples)
            {
                //log?.Debug("[   " + route.FullPath);
                //log?.Debug("[   " + group.Route.FullPath);
                log?.Debug("Group path " + group.Route.FullPath);
                if (!route.Matches(group.Route.FullPath) && !RoutePartialMatches(route, group.Route.FullPath))
                    continue;
                log?.Debug("Building! " + group.Route.FullPath);
                // Make builder if not made.
                builder ??= new SpcBuilder();

                // Add files to builder.
                var dir = group.Files.Directory.FullPath;
                foreach (var file in group.Files.Files)
                {
                    log?.Debug("Replacing " + file);
                    builder.AddOrReplaceFile(Path.Combine(dir, file));
                }
            }

            return builder != null;
        }
        private bool RoutePartialMatches(Route route, string groupPath)
        {
            int dotIndex = groupPath.LastIndexOf('.');
            if (dotIndex == -1)
                return false; // Doesn't have any archive files, don't bother with this

            int fileEnd = groupPath.IndexOf('\\', dotIndex);
            if (fileEnd == -1)
                fileEnd = groupPath.Length; // There are no children of the archive file

            return groupPath.AsSpan(0, fileEnd).Contains(route.FullPath.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }

    internal struct RouteGroupTuple
    {
        /// <summary>
        /// Route associated with this tuple.
        /// </summary>
        public Route Route;

        /// <summary>
        /// Files bound by this route.
        /// </summary>
        public DirectoryFilesGroup Files;
    }
}
