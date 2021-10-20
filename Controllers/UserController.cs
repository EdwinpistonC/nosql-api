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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IBucket _bucket;
        private readonly ICluster cluster;
        private readonly IDBHelper dbHelper;

        public UserController(ILogger<UserController> logger,
                                INamedBucketProvider bucketProvider, 
                                IClusterProvider clusterProvider,
                                IDBHelper _dbHelper)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();
            cluster = clusterProvider.GetClusterAsync().GetAwaiter().GetResult();
            dbHelper = _dbHelper;
        }

      

      
        [HttpPut]
        public async Task<object> put([FromBody]UserDto userDto)
        {

            var collection = await _bucket.CollectionAsync("User");

            // defaulting the id value to insert. New Id generation has different approaches which is not discussed here. 
            var generate_id = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
            parameters[0] = new KeyValuePair<string, object>("$Email", userDto.Email);
            var queryResult = await cluster.QueryAsync<UserDto>(@"select name, email, surname, u.`password` 
                                                                                        from `nosqldatabase`.`_default`.`User` u 
                                                                                        where u.email = $Email",
             options => options.Parameter(parameters));

            var userList = await queryResult.ToListAsync<UserDto>();
            if (userList.Count == 0)
            {
                User user = new User
                {
                    Email = userDto.Email,
                    Password=userDto.Password,
                    Surname= userDto.Surname,
                    Name = userDto.Name
                };
                await collection.InsertAsync<User>($"user_{generate_id}", user);
                return "Usuario ingresado";
            }
            else {
                KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                parameters2[0] = new KeyValuePair<string, object>("$Id", 101);
                var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                   from `nosqldatabase`.`_default`.`Code` c
                                                                    where c.id = $Id",
                                            options => options.Parameter(parameters2));

                return await queryResult2.ToListAsync();
            }
            // using the collection object insert the new airport object  
        }

        [HttpPut]
        [Route("authUser")]
        public async Task<bool> AuthUser(AuthUser authUser)
        {
            var collection = await _bucket.CollectionAsync("User");
            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[2];
            parameters[0] = new KeyValuePair<string, object>("$Email", authUser.Email);
            parameters[1] = new KeyValuePair<string, object>("$Pass", authUser.Password);

            var queryResult = await cluster.QueryAsync<User>(@"select name, email, surname, u.`password` 
                                                               from `nosqldatabase`.`_default`.`User` u 
                                                               where u.email=$Email and u.`password`=$Pass ",
             options => options.Parameter(parameters));

            var codeList = await queryResult.ToListAsync<User>();

            return codeList.Count >0;

        }

        [HttpPut]
        [Route("addRol")]
        public async Task<object> addRol([FromBody] RolDto rolDto)
        {

            var collection = await _bucket.CollectionAsync("User");

            // defaulting the id value to insert. New Id generation has different approaches which is not discussed here. 
            var generate_id = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
            parameters[0] = new KeyValuePair<string, object>("$Email", rolDto.Email);

            var queryResult = await cluster.QueryAsync<User>(@"select name, email, surname, u.`password`,u.`rols` 
                                                                                        from `nosqldatabase`.`_default`.`User` u 
                                                                                        where u.email = $Email",
             options => options.Parameter(parameters));
            var userList = (await queryResult.ToListAsync<User>());
            if (userList.Count==0 )
            {
                var collection2 = await _bucket.CollectionAsync("Code");
                KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                parameters2[0] = new KeyValuePair<string, object>("$Code", 102);
                var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                                            from `nosqldatabase`.`_default`.`Code` c 
                                                                                            where c.id = $Code",
                 options => options.Parameter(parameters2));

                var codeList = await queryResult2.ToListAsync<Code>();

                return codeList[0];
            }
            var user = userList[0];

            if (user.Password.Equals(rolDto.Password))
            {
                KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[2];
                parameters2[0] = new KeyValuePair<string, object>("$Rol", rolDto.RolName);
                parameters2[1] = new KeyValuePair<string, object>("$Email", rolDto.Email);

                var queryResult2 = await cluster.QueryAsync<User>(@"UPDATE `nosqldatabase`.`_default`.`User` SET rols = ARRAY_PUT(IFNULL(rols, []), $Rol)
                                                                    WHERE email = $Email",
                                                                  options => options.Parameter(parameters2));




                return "Rol ingresado";
            }
            else
            {
                var collection2 = await _bucket.CollectionAsync("Code");
                KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                parameters2[0] = new KeyValuePair<string, object>("$Code", 104);
                var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                                            from `nosqldatabase`.`_default`.`Code` c 
                                                                                            where c.id = $Code",
                 options => options.Parameter(parameters2));

                var codeList = await queryResult2.ToListAsync<Code>();

                return codeList[0];
            }
        }

        [HttpPut]
        [Route("removeRols")]
        public async Task<object> removeRols([FromBody] RolsDto rolsDto)
        {

            var collection = await _bucket.CollectionAsync("User");

            // defaulting the id value to insert. New Id generation has different approaches which is not discussed here. 
            var generate_id = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[1];
            parameters[0] = new KeyValuePair<string, object>("$Email", rolsDto.Email);

            var queryResult = await cluster.QueryAsync<User>(@"select name, email, surname, u.`password`,u.`rols` 
                                                                                        from `nosqldatabase`.`_default`.`User` u 
                                                                                        where u.email = $Email",
             options => options.Parameter(parameters));
            var userList = (await queryResult.ToListAsync<User>());
            if (userList.Count == 0)
            {
                var collection2 = await _bucket.CollectionAsync("Code");
                KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                parameters2[0] = new KeyValuePair<string, object>("$Code", 102);
                var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                                            from `nosqldatabase`.`_default`.`Code` c 
                                                                                            where c.id = $Code",
                 options => options.Parameter(parameters2));

                var codeList = await queryResult2.ToListAsync<Code>();

                return codeList[0];
            }
            var user = userList[0];

            if (user.Password.Equals(rolsDto.Password))
            {
                int countRol=0;

                if (user.Rols ==null)
                {
                    var collection2 = await _bucket.CollectionAsync("Code");
                    KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                    parameters2[0] = new KeyValuePair<string, object>("$Code", 103);
                    var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                                            from `nosqldatabase`.`_default`.`Code` c 
                                                                                            where c.id = $Code",
                     options => options.Parameter(parameters2));

                    var codeList = await queryResult2.ToListAsync<Code>();

                    return codeList[0];
                }

                foreach (var newRol in rolsDto.Rols)
                {
                    foreach (var currentRol in user.Rols)
                    {
                        if (currentRol.Equals(newRol))
                        {
                            countRol++;
                        }
                    }
                }
                if (countRol< rolsDto.Rols.Count)
                {
                    var collection2 = await _bucket.CollectionAsync("Code");
                    KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                    parameters2[0] = new KeyValuePair<string, object>("$Code", 103);
                    var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                                            from `nosqldatabase`.`_default`.`Code` c 
                                                                                            where c.id = $Code",
                     options => options.Parameter(parameters2));

                    var codeList = await queryResult2.ToListAsync<Code>();

                    return codeList[0];
                }
                else
                {
                    foreach (var newRol in rolsDto.Rols)
                    {
                        KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[2];
                        parameters2[0] = new KeyValuePair<string, object>("$Rol", newRol);
                        parameters2[1] = new KeyValuePair<string, object>("$Email", rolsDto.Email);

                        var queryResult2 = await cluster.QueryAsync<User>(@"UPDATE `nosqldatabase`.`_default`.`User` SET rols = ARRAY_REMOVE(IFNULL(rols, []), $Rol)
                                                                    WHERE email = $Email",
                                                                          options => options.Parameter(parameters2));
                    }


                }

                return "Roles removidos";
            }
            else
            {
                var collection2 = await _bucket.CollectionAsync("Code");
                KeyValuePair<string, object>[] parameters2 = new KeyValuePair<string, object>[1];
                parameters2[0] = new KeyValuePair<string, object>("$Code", 104);
                var queryResult2 = await cluster.QueryAsync<Code>(@"select id,description
                                                                                            from `nosqldatabase`.`_default`.`Code` c 
                                                                                            where c.id = $Code",
                 options => options.Parameter(parameters2));

                var codeList = await queryResult2.ToListAsync<Code>();

                return codeList[0];
            }
        }


    } 
}