using System;
using System.Threading.Tasks;
using Ceen;
using Ceen.Database;

namespace CeenShortLink
{
    /// <summary>
    /// Implements cached access to links
    /// </summary>
    public class LinkCache : IModuleWithSetup    
    {
        /// <summary>
        /// The database module
        /// </summary>
        private readonly LinkDatabase DB = LoaderContext.SingletonInstance<LinkDatabase>();

        /// <summary>
        /// The lookup table with known entries
        /// </summary>
        private Ceen.LRUCache<LinkEntry> FoundCache;
        /// <summary>
        /// The lookup table with non-existing links.
        /// This lookup prevents hammering the database with repeated requests for the same url 
        /// </summary>
        private Ceen.LRUCache<DateTime> NotFoundCache;

        /// <summary>
        /// The timespan to use for keeping NotFound items in cache
        /// </summary>
        public TimeSpan ExpireTimeout { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// The maximum number of items to keep in the found cache
        /// </summary>
        public int FoundCacheSize { get; set; } = 10000;
        /// <summary>
        /// The maximum number of items to keep in the notfound cache
        /// </summary>
        public int NotFoundCacheSize { get; set; } = 10000;

        /// <summary>
        /// The lock guarding the frontpage settings
        /// </summary>
        /// <returns></returns>
        private readonly AsyncLock m_lock = new AsyncLock();

        /// <summary>
        /// The frontpage settings
        /// </summary>
        private FrontpageSettings m_frontpageSettings;
        
        /// <summary>
        /// Constructs the link cache and registers the singleton
        /// </summary>
        public LinkCache()
        {
            LoaderContext.RegisterSingletonInstance(this);
        }

        /// <summary>
        /// Removes all entries for a particular path
        /// </summary>
        /// <param name="path">The path to remove entries for</param>
        /// <returns>An awaitable task</returns>
        public Task InvalidateTargetAsync(string path)
        {
            return Task.WhenAll(
                FoundCache.TryGetUnlessAsync(path, (a, b) => true),
                NotFoundCache.TryGetUnlessAsync(path, (a, b) => true)
            );
        }

        /// <summary>
        /// Helper method that returns the redirect url or null
        /// </summary>
        /// <param name="path">The path to get the redirect url for</param>
        /// <returns>The target url, or <c>null</c></returns>
        public async Task<string> GetRedirecLinkAsync(string path)
        {
            if (!FoundCache.TryGetValue(path, out var le))
            {
                var notFound = await NotFoundCache.TryGetUnlessAsync(path, (a, b) => b < DateTime.Now);
                if (notFound.Key)
                    return null;

                le = await DB.RunInTransactionAsync(db => db.SelectSingle<LinkEntry>(x => x.Match == path));
                if (le == null)
                {
                    await NotFoundCache.AddOrReplaceAsync(path, DateTime.Now.Add(ExpireTimeout));
                }
                else
                {
                    await FoundCache.AddOrReplaceAsync(path, le);
                }
            }

            if (le != null && le.Enabled && le.IsActive)
                return le.TargetUrl;
            return null;
        }

        /// <summary>
        /// Clears the currently cached instance of the frontpage settings
        /// </summary>
        /// <returns>An awaitable task</returns>
        public async Task InvalidateFrontpageSettingsAsync()
        {
            using(await m_lock.LockAsync())
                m_frontpageSettings = null;
        }

        /// <summary>
        /// Gets the current frontpage settings, attempting to use a cached
        /// version without needing any locks, and loading an instance if needed
        /// </summary>
        /// <returns>The frontpage settings</returns>
        public async Task<FrontpageSettings> GetFrontpageSettingsAsync()
        {
            var tmp = m_frontpageSettings;
            if (tmp != null)
                return tmp;

            using(await m_lock.LockAsync())
            {
                if (m_frontpageSettings != null)
                    return m_frontpageSettings;

                return m_frontpageSettings = await DB.RunInTransactionAsync(db => 
                    db.SelectItemById<FrontpageSettings>(LinkDatabase.FRONTPAGESETTINGS_KEY)
                );
            }
        }

        /// <summary>
        /// Sets up the cache tables
        /// </summary>
        public void AfterConfigure()
        {
            if (ExpireTimeout < TimeSpan.FromMinutes(1))
                throw new Exception($"The value of {ExpireTimeout} cannot be less than a minute");
            FoundCache = new LRUCache<LinkEntry>(countlimit: FoundCacheSize);
            NotFoundCache = new LRUCache<DateTime>(countlimit: NotFoundCacheSize);
        }

    }
}