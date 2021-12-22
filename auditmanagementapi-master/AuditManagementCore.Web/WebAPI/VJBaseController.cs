using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using VJLiabraries.Wrappers;

namespace AuditManagementCore.Web.Controllers
{
    [ApiController]

    public abstract class VJBaseController : ControllerBase
    {
        #region Response Methods


        [NonAction]
        public ActionResult ResponseException(Exception ex, string msg = "")
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Exception: " + ex.Message);
        }
        [NonAction]
        public ActionResult ResponseNotFound(string msg = "")
        {
            return NotFound(msg);
        }
        [NonAction]
        public ActionResult ResponseError(string msg)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, msg);
        }
        [NonAction]
        public ActionResult ResponseError(VJErrors r)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, r.Message);
        }
        [NonAction]
        public ActionResult ResponseUnAuth(string msg)
        {
            return Unauthorized(msg);
        }
        [NonAction]
        public ActionResult ResponseBad(string msg)
        {
            return BadRequest(msg);
        }
        [NonAction]
        public ActionResult ResponseSuccess(object obj)
        {
            return Ok(obj);
        }
        // same as success
        [NonAction]
        public ActionResult ResponseOK(object obj)
        {
            return Ok(obj);
        }
        [NonAction]
        public ActionResult ResponseCreated(object obj)
        {
            return Created(this.Url.ToString(), obj);
        }
        public ActionResult CustomResponseError(string msg)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, msg);
        }
        public ActionResult  AlreadyExistResponseError(string msg)
        {
            return StatusCode(StatusCodes.Status208AlreadyReported, msg);
        }
        public ActionResult UnauthorizedResponseError(string msg)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, msg);
        }

        #endregion Response Methods
    }
}
