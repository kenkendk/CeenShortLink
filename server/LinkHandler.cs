using System;
using System.Threading.Tasks;
using Ceen;

namespace CeenShortLink
{
    public class LinkHandler : IHttpModule
    {
        private readonly LinkCache Cache = LoaderContext.SingletonInstance<LinkCache>();

        public async Task<bool> HandleAsync(IHttpContext context)
        {
            var target = await Cache.GetRedirecLinkAsync(context.Request.Path);
            if (target == null)
                return false;
            
            if (context.Request.Method != "GET" && context.Request.Method != "HEAD")
                return context.SetResponseMethodNotAllowed();

            return context.SetResponseRedirect(target);
        }
    }
}