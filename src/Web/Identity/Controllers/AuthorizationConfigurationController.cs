using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Identity.Authorization.Core;
using SampleBlog.Web.Identity.Core;

namespace SampleBlog.Web.Identity.Controllers
{
    [Route("_configuration/authorization")]
    [ApiController]
    public class AuthorizationConfigurationController : ControllerBase
    {
        private readonly IClientRequestParametersProvider clientRequestParametersProvider;
        private readonly ILogger<AuthorizationConfigurationController> logger;

        public AuthorizationConfigurationController(
            IClientRequestParametersProvider clientRequestParametersProvider,
            ILogger<AuthorizationConfigurationController> logger)
        {
            this.clientRequestParametersProvider = clientRequestParametersProvider;
            this.logger = logger;
        }

        [EnableCors(Constants.ClientPolicy)]
        [HttpGet("{clientId:required}")]
        public async Task<IActionResult> Get([FromRoute] string clientId)
        {
            var clientParameters = await clientRequestParametersProvider.GetClientParametersAsync(HttpContext, clientId);
            return Ok(clientParameters);
        }
    }
}
