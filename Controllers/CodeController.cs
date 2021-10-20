using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Couchbase.Extensions.DependencyInjection;
using Couchbase;
using CouchbaseWebAPI.Common;
using Couchbase.Query;
using CouchbaseWebAPI.Models;
using System.Globalization;

namespace CouchbaseWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;
        private readonly IBucket _bucket;
        private readonly ICluster cluster;
        private readonly IDBHelper dbHelper;

        public CodeController(ILogger<CodeController> logger,
                                INamedBucketProvider bucketProvider, 
                                IClusterProvider clusterProvider,
                                IDBHelper _dbHelper)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();
            cluster = clusterProvider.GetClusterAsync().GetAwaiter().GetResult();
            dbHelper = _dbHelper;
        }
        [HttpGet]
        public async Task<IList<Code>> GetAllCode()
        {
            try
            {
                var queryResult = await cluster.QueryAsync<Code>(@"select id,description
                                                                   from `nosqldatabase`.`_default`.`Code` c");

                return await queryResult.ToListAsync<Code>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("getCodeById/{id}")]
        public async Task<Code> GetCodeById(int id)
        {
            var collection = await _bucket.CollectionAsync("Code");
            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
            parameters[0] = new KeyValuePair<string, object>("$Code", id);
            var queryResult = await cluster.QueryAsync<Code>(@"select id,description
                                                                                        from `nosqldatabase`.`_default`.`Code` c 
                                                                                        where c.id = $Code",
             options => options.Parameter(parameters));

            var codeList = await queryResult.ToListAsync<Code>();

            return codeList[0];

        }

        [HttpPut]
        public async Task<object> put([FromBody] Code code)
        {

            var collection = await _bucket.CollectionAsync("Code");

            // defaulting the id value to insert. New Id generation has different approaches which is not discussed here. 
            var generate_id = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
            parameters[0] = new KeyValuePair<string, object>("$Code", code.Id);
            var queryResult = await cluster.QueryAsync<Code>(@"select id,description
                                                                                        from `nosqldatabase`.`_default`.`Code` c 
                                                                                        where c.id = $Code",
             options => options.Parameter(parameters));

            var codeList = await queryResult.ToListAsync<Code>();
            if (codeList.Count == 0)
            {
                await collection.InsertAsync<Code>($"code_{generate_id}", code);
                return "ok";
            }
            else
            {
                return "error";
            }
            // using the collection object insert the new airport object  

        }


    } 
}