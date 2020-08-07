using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Ceen.Httpd;
using Ceen.Httpd.Logging;
using Ceen;

namespace CeenShortLinkSimple
{
    class Program
    {
        /// <summary>
        /// The links we handle
        /// </summary>
        private static Dictionary<string, string> m_links;

        /// <summary>
        /// The URL we redirect index/root requests to
        /// </summary>
        private static string m_homepageurl;

        /// <summary>
        /// The URL we redirect 404 - not found requests to
        /// </summary>
        private static string m_404url;

        /// <summary>
        /// Choose the string compare, case-sensitive or not
        /// </summary>
        private static readonly StringComparer COMPARER = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// The urls we redirect to the homepage
        /// </summary>
        private static readonly HashSet<string> m_indexUrls 
            = new HashSet<string>(COMPARER) {
                "/", "/index", "/index.htm", "/index.html"
            };

        /// <summary>
        /// Helper method to valid a url
        /// </summary>
        /// <param name="url">The url to check for validity</param>
        /// <returns><c>true</c> if the url is valid, <c>false</c> otherwise</returns>
        private static bool IsValidUrl(string url)
            => !string.IsNullOrWhiteSpace(url)
                &&
                url.StartsWith("http://") || url.StartsWith("https://");

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Validate commandline input
            if (args == null || (args.Length != 3 && args.Length != 4))
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine();
                Console.WriteLine($"{Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location)} <linkfile> <port> <redirect-homepage> [404 url]");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"File not found: {args[0]}");
                return;
            }

            if (!int.TryParse(args[1], out var port) || port <= 0 || port >= 65535)
            {
                Console.WriteLine($"Not a valid port: {args[1]}");
                return;
            }

            m_homepageurl = args[2];
            if (!IsValidUrl(m_homepageurl))
            {
                Console.WriteLine($"Not a valid redirect url: {m_homepageurl}");
                return;
            }

            if (args.Length > 3)
            {
                if (!IsValidUrl(args[3]))
                {
                    Console.WriteLine($"Not a valid redirect url: {args[3]}");
                    return;
                }

                m_404url = args[3];
            }

            // Read the links to get started
            var linkfile = Path.GetFullPath(args[0]);
            m_links = ParseLinksFiles(linkfile);

            // Check for user error
            if (m_links.Count == 0)
            {
                Console.WriteLine("No links found in file, bad format or wrong file?");
                return;
            }

            // Get shorthands to use for the filesystem watcher
            var folder = Path.GetDirectoryName(linkfile);
            var filename = Path.GetFileName(linkfile);

            // Set up the server
            var serverCancelToken = new CancellationTokenSource();
            var task = HttpServer.ListenAsync(
                new System.Net.IPEndPoint(System.Net.IPAddress.Any, port),
                false,
                new ServerConfig()
                    // Log errors to stdout
                    .AddLogger(new StdOutErrors())
                    // Handle link request
                    .AddRoute(new LinkRedirectHandler()),
                serverCancelToken.Token
            );

            // Start monitoring the file
            using(var fs = new FileSystemWatcher(folder))
            {
                // When the file changes, register a delayed update
                // The created/deleted/renamed are required
                // to support copying in a new file
                fs.Changed += (sender, args) =>
                    StartDelayedReader(linkfile, args.Name == filename);
                fs.Created += (sender, args) =>
                    StartDelayedReader(linkfile, args.Name == filename);
                fs.Deleted += (sender, args) =>
                    StartDelayedReader(linkfile, args.Name == filename);
                fs.Renamed += (sender, args) =>
                    StartDelayedReader(linkfile, args.Name == filename || args.OldName == filename);

                fs.EnableRaisingEvents = true;

                Console.WriteLine($"Serving {m_links.Count} links, kill process to stop ...");
                task.Wait();  // Wait forever            
            }
        }

        /// <summary>
        /// Helper method to create a delayed reader
        /// </summary>
        /// <param name="linkfile">The file to read from</param>
        /// <param name="condition">A condition used to flag if the reader should start or not</param>
        private static void StartDelayedReader(string linkfile, bool condition = true)
        {
            // Skip if the condition is not satisfied
            if (!condition)
                return;

            // Create new task to kick in after 1 second
            var task = new CancelAbleDelayedTask(
                () => {
                    try
                    {
                        // Read the file without blocking
                        // Reading to a temp variable makes it safe if multiple
                        // readers are active concurrently
                        var tmp = ParseLinksFiles(linkfile);

                        // Update without locking, last reader wins                                  
                        var old = System.Threading.Interlocked.Exchange(ref m_links, tmp);
                        if (old.Count != m_links.Count)
                            Console.WriteLine($"Now serving {m_links.Count} links (change: {m_links.Count - old.Count})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse link file: {ex.Message}");
                    }
                }, TimeSpan.FromSeconds(1)
            );

            // Lock-free update, last scheduled updater wins
            var prev = System.Threading.Interlocked.Exchange(ref m_linkfileUpdater, task);
            if (prev != task)
                prev.Dispose(); // Stop the previous task            
        }

        /// <summary>
        /// Reads all link-redirect lines from the given file
        /// </summary>
        /// <param name="path">The file to read</param>
        /// <returns>The links</returns>
        private static Dictionary<string, string> ParseLinksFiles(string path)
        {
            /* Assume each line has a format like:
               
                /xyz https://example.com/abc
                /123 https://example.com/def

            */

            return File.ReadAllLines(path)
                // Check for valid lines only
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.StartsWith("/"))
                // Look for a space as an argument separator
                .Select(x => x.Split(" ", 2))
                // Check for valid content
                .Where(x => x != null && x.Length == 2)
                .Where(x => IsValidUrl(x.Last()))
                // Use the first part as the key, second as the value
                .GroupBy(x => x.First().Trim(), x => x.Last())
                // In case of duplicates, do not crash but return one
                .ToDictionary(x => x.Key, x => x.First().Trim(), COMPARER);
        }

        /// <summary>
        /// Helper class to allow us to cancel a pending task
        /// </summary>
        private class CancelAbleDelayedTask : IDisposable
        {
            private readonly CancellationTokenSource m_tcs = new CancellationTokenSource();
            private readonly Task m_task;

            public CancelAbleDelayedTask(Action action, TimeSpan delay)
            {
                m_task = 
                    Task.Delay(delay, m_tcs.Token)
                    .ContinueWith(x => action(), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            public void Dispose()
            {
                m_tcs.Cancel();
            }
        }

        // Reduce hammering if the link file is repeatedly updated
        // The dummy task here is to avoid having a null value
        private static CancelAbleDelayedTask m_linkfileUpdater = 
            new CancelAbleDelayedTask(() => {}, TimeSpan.FromSeconds(1));

        /// <summary>
        /// The main class handling http requests
        /// </summary>
        private class LinkRedirectHandler : IHttpModule
        {
            // Task that signals we handled the request
            // used to return a task since we do not use async here
            private Task<bool> m_handled = Task.FromResult(true);

            public Task<bool> HandleAsync(IHttpContext context)
            {
                // All responses are dynamic
                context.Response.SetNonCacheable();

                // Basic validation
                if (context.Request.Method != "GET" && context.Request.Method != "HEAD")
                {
                    context.Response.SetStatus(HttpStatusCode.MethodNotAllowed);
                    return m_handled;
                }
                
                // Check for index requests
                if (m_indexUrls.Contains(context.Request.Path))
                {
                    context.Response.Redirect(m_homepageurl);
                    return m_handled;
                }

                // Check for redirect targets
                m_links.TryGetValue(context.Request.Path, out var target);
                if (!string.IsNullOrWhiteSpace(target))
                {
                    context.Response.Redirect(target);
                    return m_handled;
                }

                // Handle not-found
                if (string.IsNullOrWhiteSpace(m_404url))
                {
                    context.Response.SetStatus(HttpStatusCode.NotFound);
                    return m_handled;
                }
                
                context.Response.Redirect(m_404url);
                return m_handled;
            }
        }
    }
}
