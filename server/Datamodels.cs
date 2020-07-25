using System.Linq;
using System;
using Ceen.Database;

namespace CeenShortLink
{
    /// <summary>
    /// Custom validation for the Match field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LinkEntryMatchValidatorAttribute : ValidationBaseAttribute
    {
        public override void Validate(object _value)
        {
            var value = _value as string;
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("/"))
                throw new ValidationException("The match entry must start with a forward slash");
            if (value == "/admin")
                throw new ValidationException("The match entry cannot use /admin");
            if (value.Count(x => x == '/') != 1)
                throw new ValidationException("The match entry cannot have slashes (except the starting)");
            if (value.Length > 200)
                throw new ValidationException("The match entry cannot be more than 200 characters");
        }
    }

    /// <summary>
    /// Custom validation for the TargetUrl field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LinkEntryTargetUrlValidatorAttribute : ValidationBaseAttribute
    {
        public override void Validate(object _value)
        {
            var value = _value as string;
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("The targeturl cannot be empty");
            if (!value.StartsWith("http://") && !value.StartsWith("https://"))
                throw new ValidationException("The targeturl must start with either http:// or https://");
            if (value.Length > 1024)
                throw new ValidationException("The targeturl cannot be more than 1024 characters");
        }
    }

    /// <summary>
    /// The entry for a single short-link registration item
    /// </summary>
    public class LinkEntry
    {
        /// <summary>
        /// The unique ID for the link
        /// </summary>
        [PrimaryKey]
        public long ID;
        /// <summary>
        /// The ID of the user owning the link
        /// </summary>
        public string OwnerID;
        /// <summary>
        /// An optional name to use for the link in logs
        /// </summary>
        [StringLengthRule(0, 500)]
        public string Name;
        /// <summary>
        /// The path to match
        /// </summary>
        [LinkEntryMatchValidator]
        public string Match;
        /// <summary>
        /// The url to redirect to
        /// </summary>
        [LinkEntryTargetUrlValidator]
        public string TargetUrl;
        /// <summary>
        /// When the link expires
        /// </summary>
        public DateTime Expires;
        /// <summary>
        /// When the link becomes valid
        /// </summary>
        public DateTime ValidFrom;
        /// <summary>
        /// A flag indicating if the link is enabled
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// When the link was created
        /// </summary>
        [CreatedTimestamp]
        public DateTime Created;
        /// <summary>
        /// When the link was last updated
        /// </summary>
        [ChangedTimestamp]
        public DateTime Updated;

        /// <summary>
        /// Flag to indicate if the item is active
        /// </summary>
        [Ignore]
        public bool IsActive
        {
            get
            {
                var now = DateTime.Now;
                return
                    (ValidFrom.Ticks == 0 || now >= ValidFrom)
                    &&
                    (Expires.Ticks == 0 || now < Expires);
            }
            set
            {
            }
        }        
    }

    /// <summary>
    /// Record of a link being accessed
    /// </summary>
    public class LinkEntryUsage
    {
        /// <summary>
        /// The ID of the usage entry
        /// </summary>
        [PrimaryKey]
        public long ID;
        /// <summary>
        /// The ID of the link being accessed
        /// </summary>
        public long LinkID;
        /// <summary>
        /// The ID of the logged request
        /// </summary>
        public string RequestID;
        /// <summary>
        /// The time when the link was accessed
        /// </summary>
        [CreatedTimestamp]
        public DateTime When;     
    }

    /// <summary>
    /// The possible modes for the front page
    /// </summary>
    public enum FrontpageMode
    {
        /// <summary>
        /// Using a markdown renderer to produce HTML output
        /// </summary>
        Markdown,
        /// <summary>
        /// Using a frontpage redirect
        /// </summary>
        Redirect
    }

    /// <summary>
    /// Helper class for the front page settings
    /// </summary>
    public class FrontpageSettings
    {
        /// <summary>
        /// The fixed primary key for the entry
        /// </summary>
        [PrimaryKey]
        [Newtonsoft.Json.JsonIgnore]
        public string ID;

        /// <summary>
        /// The mode to use
        /// </summary>
        public FrontpageMode Mode;
        /// <summary>
        /// The url to redirect to
        /// </summary>
        public string RedirectUrl;
        /// <summary>
        /// A flag indicating if the redirect is performed internally,
        /// meaning invisible to the client
        /// </summary>
        public bool InternalRedirect;

        /// <summary>
        /// Helper field to store pre-generated html
        /// </summary>
        [Ignore]
        [Newtonsoft.Json.JsonIgnore]
        public string RenderedHtml;
    }
}