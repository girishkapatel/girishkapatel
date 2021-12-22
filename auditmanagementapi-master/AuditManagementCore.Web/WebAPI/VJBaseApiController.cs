using AuditManagementCore.MongoDb;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using VJLiabraries.GenericRepository;

namespace AuditManagementCore.Web.Controllers
{

    public abstract class VJBaseApiController : VJBaseController
    {

        #region Get Actions

        // get all from repo
        public ActionResult GetAll<T>(IMongoGenericRepository<T> _api) where T : class
        {
            var tList = _api.GetAll();
            if (tList == null)
            {
                return ResponseNotFound();
            }
            return ResponseOK(tList);
        }

        public ActionResult GetByID<T>(IMongoGenericRepository<T> _api, string id) where T : class
        {
            var e = _api.GetByID(id.ToString());

            if (e != null)
                return ResponseOK(e);
            else
                return ResponseNotFound();
        }

        #endregion Get Actions

        #region Insert, Update and Delete Actions

        public ActionResult Insert<T>(IMongoGenericRepository<T> _api, T e) where T : class
        {
            try
            {
                _api.Insert(e);
                return this.ResponseCreated(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }

        }

        internal ActionResult Update<T>(IMongoGenericRepository<T> _api, T tValue) where T : class
        {
            try
            {
                _api.Update(tValue);
                return this.ResponseCreated(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }

        public ActionResult Update<T>(IMongoGenericRepository<T> _api, T e, int id) where T : class
        {

            try
            {
                _api.Update(e);
                return this.ResponseCreated(_api.GetByID(id));
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }

        public ActionResult Delete<T>(IMongoGenericRepository<T> _api, T e, int id) where T : class
        {

            try
            {
                _api.Delete(e);
                return this.ResponseCreated(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return ResponseException(ex);
            }
        }
        #endregion Insert, Update and Delete Actions

    }
}
