using System;
using System.Threading.Tasks;
using Ceen.Database;

namespace CeenShortLink
{
    /// <summary>
    /// Database connection that holds the links
    /// </summary>
    public class LinkDatabase : Ceen.Extras.DatabaseBackedModule
    {
        /// <summary>
        /// The key used for the frontpage settings element
        /// </summary>
        public const string FRONTPAGESETTINGS_KEY = "root";

        /// <summary>
        /// The types that are persisted in the database
        /// </summary>
        protected override Type[] UsedTypes => new [] {
            typeof(LinkEntry),
            typeof(LinkEntryUsage),
            typeof(FrontpageSettings)
        };

        public override void AfterConfigure() 
        {
            base.AfterConfigure();
            Ceen.LoaderContext.RegisterSingletonInstance(this);

            var con = base.m_con.UnguardedConnection;
            
            // Make sure we have a frontpage settings element
            var fps = con.SelectItemById<FrontpageSettings>(FRONTPAGESETTINGS_KEY);
            if (fps == null)
                con.InsertItem(new FrontpageSettings() {
                    ID = LinkDatabase.FRONTPAGESETTINGS_KEY,
                    Mode = FrontpageMode.Markdown                    
                });

            // Inject a default message, if there is no landing page
            var textcon = Ceen.LoaderContext.SingletonInstance<Ceen.PaaS.DatabaseInstance>();
            textcon.RunInTransactionAsync(db => {
                var el = db.SelectItemById<Ceen.PaaS.Database.TextEntry>(Ceen.PaaS.TextConstants.LandingPageContents);
                if (el == null)
                    db.InsertOrIgnoreItem(new Ceen.PaaS.Database.TextEntry() {
                        ID = Ceen.PaaS.TextConstants.LandingPageContents,
                        Language = "en",
                        Text = 
@"# Welcome to the short-link service.

The short-link service provides url-shortenings.

You can read our [privacy policy](/privacy) and [terms of service](/tos).

If you are a registered user of the service, you can also [log in to the dashboard](/admin)."
                    });
            }).Wait();
        }


    }
}