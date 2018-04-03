namespace Services.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Contracts.Services.Dependency;
    using Microsoft.Extensions.Logging;
    using Shared.WebApi.Context;

    [RoutePrefix("api")]
    public class DefaultController : ApiController
    {
        private readonly IDependency dependency;
        private readonly ILogger logger;

        public DefaultController()
        {
        }

        public DefaultController(IDependency dependency, ILoggerFactory loggerFactory)
            : this()
        {
            this.dependency = dependency;
            logger = loggerFactory.CreateLogger<DefaultController>();
        }

        [Route("users")]
        public async Task<IEnumerable<User>> Get()
        {
            logger.LogTrace("Starting {method} controller method", "Get");
            return await dependency.GetUsers();
        }

        [Route("users/{id}")]
        public async Task<User> Get(string id)
        {
            logger.LogTrace("Starting {method}/{id} controller method", "Get", id);
            return await dependency.GetUser(id);
        }

        [Route("users")]
        public async Task<User> Post([FromBody]User user)
        {
            logger.LogTrace("Starting {method}/{id} controller method", "Post", user.Id);
            return await dependency.AddUser(user);
        }

        [Route("users/{id}")]
        public async Task Put(string id, [FromBody]User user)
        {
            user.Id = id;

            logger.LogTrace("Starting {method}/{id} controller method", "Put", user.Id);
            await dependency.UpdateUser(user);
        }

        [Route("users/{id}")]
        public async Task Delete(string id)
        {
            logger.LogTrace("Starting {method}/{id} controller method", "Delete", id);
            await dependency.DeleteUser(id);
        }

        [Route("fail")]
        [HttpGet]
        // Operation calls service that constantly fails
        public async Task Fail()
        {
            logger.LogTrace("Starting {method} controller method", "Fail");
            await dependency.Fail();
        }

        [Route("operation")]
        [OperationIdQueryString("operation", "operationId")]
        [HttpGet]
        // OperationId from the query string will be used if provided api/operation?operationId={operationId}
        public async Task<string> Operation()
        {
            logger.LogTrace("Starting {method} controller method", "Operation");
            return await Task.FromResult<string>(Activity.Current.RootId);
        }
    }
}
