
using System.Threading.Tasks;
using Ceen;
using Ceen.Database;
using Ceen.Mvc;
using Ceen.PaaS.AdminHandlers;

namespace CeenShortLink
{
    [RequireHandler(typeof(Ceen.PaaS.Services.AdminRequiredHandler))]
    public class AdminFrontpageSettingsHandler : Controller, IAdminAPIv1
    {
        /// <summary>
        /// The database where we find link information
        /// </summary>
        private readonly LinkDatabase DB = LoaderContext.SingletonInstance<LinkDatabase>();
        
        /// <summary>
        /// Cache for invalidating the frontpage contents on updating
        /// </summary>
        private readonly LinkCache Cache = LoaderContext.SingletonInstance<LinkCache>();

        [HttpGet]
        [Ceen.Mvc.Name("index")]
        public async Task<IResult> Get()
        {
            return Json(await DB.RunInTransactionAsync(db =>
                 db.SelectItemById<FrontpageSettings>(LinkDatabase.FRONTPAGESETTINGS_KEY)
            ));
        }

        [HttpPut]
        [Ceen.Mvc.Name("index")]
        public async Task<IResult> Put(FrontpageSettings settings)
        {
            if (settings == null)
                return BadRequest;

            settings.ID = LinkDatabase.FRONTPAGESETTINGS_KEY;
            if (settings.Mode == FrontpageMode.Redirect)
            {
                if (string.IsNullOrWhiteSpace(settings.RedirectUrl))
                    return Status(BadRequest, "Redirect requires a target url");

                if (settings.InternalRedirect && !settings.RedirectUrl.StartsWith("/"))
                    return Status(BadRequest, "Internal redirect urls must start with a forward slash");

                if (settings.InternalRedirect && !System.Uri.IsWellFormedUriString(settings.RedirectUrl, System.UriKind.Relative))
                    return Status(BadRequest, "Malformed internal redirect url");

                if (!settings.InternalRedirect && !System.Uri.IsWellFormedUriString(settings.RedirectUrl, System.UriKind.Absolute))
                    return Status(BadRequest, "Malformed absolute redirect url");
            }

            var res = Json(await DB.RunInTransactionAsync(db =>
                 db.UpdateItem<FrontpageSettings>(settings)                 
            ));
            await Cache.InvalidateFrontpageSettingsAsync();
            return res;
        }
    }
}