﻿using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Codit.LevelTwo.Controllers.v1
{
    [Route("world-cup/v1/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        ///     Get Health
        /// </summary>
        /// <remarks>Provides an indication about the health of the runtime</remarks>
        [HttpGet(Name = "Health_Get")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Runtime is up and running in a healthy state")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Runtime is not healthy")]

        public IActionResult Get()
        {
            return Ok();
        }
    }
}