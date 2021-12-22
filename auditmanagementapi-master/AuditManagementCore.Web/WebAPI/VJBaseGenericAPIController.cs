using AuditManagementCore.MongoDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuditManagementCore.Web.Controllers
{
    [Authorize]
    public abstract class VJBaseGenericAPIController<T> : VJBaseApiController where T : class
    {
        protected IMongoGenericRepository<T> _api;
        private bool _disposed;

        public VJBaseGenericAPIController(IMongoGenericRepository<T> api)
        {
            // for debug
            _api = api;
        }

        public VJBaseGenericAPIController() : base() { }

        [HttpGet]
        [Route("")]
        public virtual ActionResult GetAll()
        {
            try
            {
                return base.GetAll<T>(_api);
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }

        // get the entity based on string key
        [HttpGet]
        [Route("{id}")]
        public virtual ActionResult GetByID(string id)
        {
            try
            {
                return base.GetByID<T>(_api, id);
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }

        // update
        [HttpPut]
        [Route("")]
        public virtual ActionResult Put([FromBody]T tValue)
        {
            try
            {
                return base.Update<T>(_api, tValue);
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }

        //insert
        [HttpPost]
        [Route("")]
        public virtual ActionResult Post([FromBody]T e)
        {
            try
            {
                return base.Insert(_api, e);

            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }

        [HttpDelete]
        [Route("{id}/{userid}")]
        public virtual ActionResult Delete(string id, string userid)
        {
            try
            {
                _api.Delete(id);
                return ResponseSuccess(new JsonResult(id));
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }
      
    }
}