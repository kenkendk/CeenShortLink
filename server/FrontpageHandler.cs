using Ceen;
using Ceen.Mvc;
using Ceen.PaaS.API;
using Ceen.Extras;
using System.Threading.Tasks;

namespace CeenShortLink
{
    public class FrontpageHandler : IHttpModule, IAPIv1
    {
        /// <summary>
        /// Cache for invalidating the frontpage contents on updating
        /// </summary>
        private readonly LinkCache Cache = LoaderContext.SingletonInstance<LinkCache>();

        /// <summary>
        /// The database for reading the frontpage markdown data
        /// </summary>
        private readonly Ceen.PaaS.DatabaseInstance DB = LoaderContext.SingletonInstance<Ceen.PaaS.DatabaseInstance>();

        public async Task<bool> HandleAsync(IHttpContext context)
        {
            if (context.Request.Method != "GET" && context.Request.Method != "HEAD")
                return context.Response.SetStatus(HttpStatusCode.MethodNotAllowed);

            var item = await Cache.GetFrontpageSettingsAsync();
            if (item.Mode == FrontpageMode.Redirect)
            {
                if (item.InternalRedirect)
                    context.Response.InternalRedirect(item.RedirectUrl);
                else
                    context.Response.Redirect(item.RedirectUrl);

                return true;
            }

            // Use pre-rendered html, or generate if required
            if (string.IsNullOrWhiteSpace(item.RenderedHtml))
            {
                var mk = await DB.RunInTransactionAsync(db => 
                    Ceen.PaaS.Services.TextHelper.GetTextFromDb(db, Ceen.PaaS.TextConstants.LandingPageContents, null)
                );
                item.RenderedHtml = Ceen.PaaS.Services.MarkdownRenderer.RenderAsHtml(mk);
            }

            await context.Response.WriteAllAsync(item.RenderedHtml, "text/html");
            return true;
        }
    }
}