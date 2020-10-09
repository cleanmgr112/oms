using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Core
{
    public partial interface IWebHelper
    {
        /// <summary>
        /// Get URL referrer
        /// </summary>
        /// <returns>URL referrer</returns>
        string GetUrlReferrer();

        /// <summary>
        /// Get context IP address
        /// </summary>
        /// <returns>URL referrer</returns>
        string GetCurrentIpAddress();

        ///// <summary>
        ///// Gets this page name
        ///// </summary>
        ///// <param name="includeQueryString">Value indicating whether to include query strings</param>
        ///// <returns>Page name</returns>
        //string GetThisPageUrl(bool includeQueryString);
    }
}
