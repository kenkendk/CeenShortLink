using System;
using System.Threading.Tasks;
using Ceen;
using Ceen.Extras;
using Ceen.Mvc;
using Ceen.Database;
using System.Collections.Generic;

namespace CeenShortLink
{
    /// <summary>
    /// Handler for administration of links.
    /// This handler is slightly more complex because we mount
    /// it as a non-admin endpoint, so regular users can manage their own links
    /// </summary>
    public class AdminLinkHandler : Ceen.Extras.CRUDHelper<long, LinkEntry>, Ceen.PaaS.API.IAPIv1
    {
        /// <summary>
        /// The database with the links
        /// </summary>
        protected override DatabaseBackedModule Connection 
            => Ceen.LoaderContext.SingletonInstance<LinkDatabase>();

        /// <summary>
        /// The cache we invalidate
        /// </summary>
        private readonly LinkCache Cache = LoaderContext.SingletonInstance<LinkCache>();

        protected override AccessType[] AllowedAccess 
            => FullAccess;

        /// <summary>
        /// Custom authorization, where we check for a logged in user
        /// </summary>
        /// <param name="type">The requested access type</param>
        /// <param name="id">The ID of the requested item</param>
        /// <returns>An awaitable task</returns>
        public override async Task Authorize(AccessType type, long id)
        {
            if (string.IsNullOrWhiteSpace(Context.UserID))
                throw new HttpException(HttpStatusCode.Unauthorized);
            await base.Authorize(type, id);
        }

        /// <summary>
        /// Hooks into queries sent to the database, such that a non-admin
        /// user can only see their own link entries
        /// </summary>
        /// <param name="type">The query type</param>
        /// <param name="id">The ID of the item to query</param>
        /// <param name="q">The query</param>
        /// <returns>The potentially modified query</returns>
        public override async Task<Query<LinkEntry>> OnQueryAsync(AccessType type, long id, Query<LinkEntry> q)
        {
            if (type != AccessType.Add && !await Ceen.PaaS.Services.AdminHelper.IsAdminAsync(Context.UserID))
                return q.Where(x => x.OwnerID == Context.UserID);

            return q;
        }

        /// <summary>
        /// Ensures that non-admin users can only insert links with themselves as owner
        /// </summary>
        /// <param name="item">The item to insert</param>
        /// <returns>An awaitable task</returns>
        protected override async Task BeforeInsertAsync(LinkEntry item)        
        {
            if (string.IsNullOrWhiteSpace(item.OwnerID) || !await Ceen.PaaS.Services.AdminHelper.IsAdminAsync(Context.UserID))
                item.OwnerID = Context.UserID;
        }

        /// <summary>
        /// Ensures that non-admin users cannot change owner of items they edit
        /// </summary>
        /// <param name="key">The entry key</param>
        /// <param name="values">The values sent from the client</param>
        /// <returns>An awaitable task</returns>
        protected override async Task BeforeUpdateAsync(long key, Dictionary<string, object> values)
        {
            if (!await Ceen.PaaS.Services.AdminHelper.IsAdminAsync(Context.UserID))
                values[nameof(LinkEntry.OwnerID)] = Context.UserID;
        }
                
        // Invalidate the cache on insert
        public override async Task<IResult> Post(LinkEntry item)
        {
            var res = await base.Post(item);
            var up = ExtractResult(res);
            if (up != null)
                await Cache.InvalidateTargetAsync(up.Match);
            return res;
        }

        // Invalidate the cache on update
        public override async Task<IResult> Patch(long id, System.Collections.Generic.Dictionary<string, object> values)
        {
            // TODO: We could update right before the patch call, and then we would invalidate the wrong cache entry
            var prev = await Connection.RunInTransactionAsync(db => db.SelectItemById<LinkEntry>(id));
            var res = await base.Patch(id, values);
            var up = ExtractResult(res);
            if (prev != null)
                await Cache.InvalidateTargetAsync(prev.Match);
            if (up != null)
                await Cache.InvalidateTargetAsync(up.Match);

            return res;
        }

        // Invalidate the cache on delete
        public override async Task<IResult> Delete(long id)
        {
            // TODO: We could update right before the delete call, and then we would invalidate the wrong cache entry
            var prev = await Connection.RunInTransactionAsync(db => db.SelectItemById<LinkEntry>(id));
            var res = await base.Delete(id);
            if (prev != null)
                await Cache.InvalidateTargetAsync(prev.Match);
            return res;
        }
    }
}