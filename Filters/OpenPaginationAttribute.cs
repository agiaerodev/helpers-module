using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Filters
{
    /// <summary>
    /// The OpenPaginationAttribute class is used to modify the UrlRequestBase useDefaultPagination property in order to make the listing not use default take and page
    /// listing. Can be used to list the entire data from a table 
    /// </summary>
    public class OpenPaginationAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
           
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Modify UrlRequestBase useDefaultPagination object directly
            if (context.ActionArguments.ContainsKey("urlRequestBase"))
            {
                var urlRequestBase = context.ActionArguments["urlRequestBase"] as dynamic;
                if (urlRequestBase != null)
                {
                    urlRequestBase.useDefaultPagination = false;
                }
            }
        }

    }
}
