using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ceen;
using Ceen.Database;
using Ceen.Mvc;

namespace CeenShortLink
{
    /// <summary>
    /// Override of the normal PaaS dashboard handler,
    /// adding link specific data to the outputs
    /// </summary>
    [RequireHandler(typeof(Ceen.PaaS.Services.AdminRequiredHandler))]
    public class AdminDashboardHandler : Ceen.PaaS.AdminHandlers.DashboardStatsHandler
    {
        /// <summary>
        /// The database where we find link information
        /// </summary>
        private readonly LinkDatabase LinkDB = Ceen.LoaderContext.SingletonInstance<LinkDatabase>();

        /// <summary>
        /// Helper class for reading/reporting the top-visited links
        /// </summary>
        private class LinkEntryCount
        {
            // We only assign these via reflection
            #pragma warning disable 0649

            public long ID;
            public string Match;
            public string TargetUrl;
            public long Count;
            
            #pragma warning restore 0649            
        }

        /// <summary>
        /// Override the dashboard overview response to report link-specific data
        /// </summary>
        /// <param name="timelimit">The timelimit used for reporting on the dashboard</param>
        /// <returns>A JSON serializable object</returns>
        protected override async Task<Dictionary<string, object>> IndexResultsAsync(DateTime timelimit)
        {
            var dbtask = LinkDB.RunInTransactionAsync(db => 
                new {
                    LinksTotal = db.SelectCount<LinkEntry>(QueryUtil.Empty),
                    NewLinks = db.SelectCount<LinkEntry>(x => x.Created > timelimit),
                    Clicks = db.SelectCount<LinkEntryUsage>(x => x.When > timelimit),
                    // GroupBy not supported by Ceen.Database
                    MostPopular = db.CustomQuery<LinkEntryCount>(
                        $"SELECT {nameof(LinkEntry.ID)} AS {nameof(LinkEntryCount.ID)}, {nameof(LinkEntry.Match)} AS {nameof(LinkEntryCount.Match)}, {nameof(LinkEntry.TargetUrl)} AS {nameof(LinkEntryCount.TargetUrl)}, CNT as {nameof(LinkEntryCount.Count)} " +
                        $"FROM {nameof(LinkEntry)} A, (" +
                            $"SELECT {nameof(LinkEntryUsage.LinkID)}, COUNT(*) AS CNT" +
                            $" FROM {nameof(LinkEntryUsage)}" +
                            $" GROUP BY {nameof(LinkEntryUsage.LinkID)}" +
                            $" ORDER BY CNT DESC LIMIT 5" +
                        $") B WHERE A.{nameof(LinkEntry.ID)} = B.{nameof(LinkEntryUsage.LinkID)}"                    
                    ).ToArray()
                }
            );

            // Run concurrently as they query two different databases
            var dict = await base.IndexResultsAsync(timelimit);
            // Patch with results
            dict["Links"] = await dbtask;

            // Return the combined results
            return dict;
        }

        /// <summary>
        /// Adds the option to obtain graph data for links
        /// </summary>
        /// <param name="req">The request</param>
        /// <param name="ranges">The computed ranges to use</param>
        /// <returns>A response object</returns>
        protected override async Task<IResult> CreateGraphResponse(GraphQuery req, TimeRange[] ranges)
        {
            var type = req?.Type?.ToLowerInvariant().Trim();
            if (type == "links")
            {
                return Json(await LinkDB.RunInTransactionAsync(db => 
                    new {
                        Clicks = 
                            ranges.Select(t =>
                                db.SelectCount<LinkEntryUsage>(x => x.When > t.From && x.When <= t.To)
                            ).ToArray(),
                        NewLinks = 
                            ranges.Select(t =>
                                db.SelectCount<LinkEntry>(x => x.Created > t.From && x.Created <= t.To)
                            ).ToArray(),
                    }));                
            }

            // Anything non-link related is passed on, 
            // including potential non-supported requests
            return await base.CreateGraphResponse(req, ranges);
        }
    }
}