// Copyright (c) Microsoft. All rights reserved.
namespace TestResultCoordinator.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class TestOperationResultController : Controller
    {
        // POST api/TestOperationResult
        [HttpPost]
        public async Task<StatusCodeResult> PostAsync(TestOperationResult result)
        {
            try
            {
                if (Enum.TryParse(result.Type, out Microsoft.Azure.Devices.Edge.ModuleUtil.ResultType resultType))
                {
                    await TestOperationResultStorage.AddResultAsync(result);
                }
                else
                {
                    return this.StatusCode((int)HttpStatusCode.BadRequest);
                }
            }
            catch (InvalidDataException)
            {
                return this.StatusCode((int)HttpStatusCode.BadRequest);
            }

            return this.StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
