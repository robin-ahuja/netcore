using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrgAPI
{
    //AFTER THIS CLASS, no need to write try-catch block
    public class ExceptionBuilder : IExceptionFilter
    {
        public void OnException(ExceptionContext context) {
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = 500;
            response.ContentType = "application/json";
            context.Result = new ObjectResult(context.Exception.Message);

        }
    }
}
