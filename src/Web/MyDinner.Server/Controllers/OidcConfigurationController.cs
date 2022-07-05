using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Identity.Authorization.Core;
using SampleBlog.Web.Application.MyDinner.Server.Core;

namespace SampleBlog.Web.Application.MyDinner.Server.Controllers
{
    [Route("_configuration/authorization")]
    [ApiController]
    public class OidcConfigurationController : ControllerBase
    {
        private readonly IClientRequestParametersProvider clientRequestParametersProvider;
        private readonly ILogger<OidcConfigurationController> logger;

        public OidcConfigurationController(
            IClientRequestParametersProvider clientRequestParametersProvider,
            ILogger<OidcConfigurationController> logger)
        {
            this.clientRequestParametersProvider = clientRequestParametersProvider;
            this.logger = logger;
        }

        [EnableCors(Constants.ClientPolicy)]
        [HttpGet("{clientId:required}")]
        public async Task<IActionResult> GetClientRequestParameters([FromRoute] string clientId)
        {
            var clientParameters = await clientRequestParametersProvider.GetClientParametersAsync(HttpContext, clientId);
            return Ok(clientParameters);
        }
    }
}