using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Voice.Shared.Core;
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Interfaces;
using Voice.Shared.Core.Models;

namespace Voice.Server.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class VoiceController : ApiController
    {
        string _controller_key;

       
        static IServiceFactory _ServiceFactory;

        #region granttype
        [HttpPost]
        [ActionName("AddGrantType")]
        [ResponseType(typeof(grant_typeC))]
        public IHttpActionResult AddGrantType(dto_grant_type_newC _dto)
        {
            var _grantService = _ServiceFactory.GetService<IGrantTypeService>();
            _grantService.controller_key = _controller_key;
            var _user = _grantService.AddNew(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_user);

        }
        //
        [HttpGet]
        [ActionName("GetAllGrantTypes")]
        [ResponseType(typeof(List<grant_typeC>))]
        public IHttpActionResult GrantTypes()
        {
            var _grantService = _ServiceFactory.GetService<IGrantTypeService>();
            _grantService.controller_key = _controller_key;
            var _gp_list = _grantService.GetAll().Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_gp_list);

        }
        //
        [HttpPost]
        [ActionName("EditGrantType")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult EditGrantType(dto_grant_type_updateC _dto)
        {
            var _grantTypeService = _ServiceFactory.GetService<IGrantTypeService>();
            _grantTypeService.controller_key = _controller_key;
            var _updated = _grantTypeService.Update(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }
        //
        [HttpGet]
        [ActionName("GetAllTargetGPByGrantType")]
        [ResponseType(typeof(List<target_groupC>))]
        public IHttpActionResult GetAllTargetGPByGrantType([FromUri] int grant_type_id)
        {
            var _tg_Service = _ServiceFactory.GetService<ITargetGroupService>();
            _tg_Service.controller_key = _controller_key;
            var _list = _tg_Service.GetAllTargetGroupGrantType(grant_type_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_list);

        }
        //
        [HttpGet]
        [ActionName("GetAllGranteesByGrantType")]
        [ResponseType(typeof(List<granteeC>))]
        public IHttpActionResult GetAllGranteesByGrantType([FromUri] int grant_type_id)
        {
            var _Service = _ServiceFactory.GetService<IGranteeService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllGranteesByGrantType(grant_type_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_list);
        }
        [HttpGet]
        [ActionName("GetAllUsersByGrantType")]
        [ResponseType(typeof(List<dto_voice_userC>))]
        public IHttpActionResult GetAllUsersByGrantType([FromUri] int grant_type_id, long fs_timestamp)
        {
            var _Service = _ServiceFactory.GetService<IVoiceUserService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetUsersByGrantType(grant_type_id, fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }
            return Ok(_list);

        }
        //
        [HttpPost]
        [ActionName("DeleteGrantType")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult DeleteGrantType([FromBody]int id)
        {
            var _grantService = _ServiceFactory.GetService<IGrantTypeService>();
            _grantService.controller_key = _controller_key;
            var _updated = _grantService.Delete(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }

        #endregion
        #region Target_Area
        [HttpPost]
        [ActionName("AddTargetGroup")]
        [ResponseType(typeof(target_groupC))]
        public IHttpActionResult AddTargetGroup(dto_target_group_newC _dto)
        {
            var _groupService = _ServiceFactory.GetService<ITargetGroupService>();
            _groupService.controller_key = _controller_key;
            var _user = _groupService.AddNew(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_user);

        }
        //
        [HttpGet]
        [ActionName("GetTargetGroups")]
        [ResponseType(typeof(List<target_groupC>))]
        public IHttpActionResult TargetGroups()
        {
            var _groupService = _ServiceFactory.GetService<ITargetGroupService>();
            _groupService.controller_key = _controller_key;
            var _gp_list = _groupService.GetAll().Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_gp_list);

        }
        //
        [HttpGet]
        [ActionName("GetTargetGroupsWithFs")]
        [ResponseType(typeof(List<target_groupC>))]
        public IHttpActionResult GetAllItems([FromUri]long fs_timestamp)
        {
            var _groupService = _ServiceFactory.GetService<ITargetGroupService>();
            _groupService.controller_key = _controller_key;
            var _gp_list = _groupService.GetAll(fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_gp_list);
        }
        //
        [HttpPost]
        [ActionName("EditTargetGroup")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult EditTargetGroup(dto_target_group_updateC _dto)
        {
            var _groupService = _ServiceFactory.GetService<ITargetGroupService>();
            _groupService.controller_key = _controller_key;
            var _updated = _groupService.Update(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }
        //
        [HttpPost]
        [ActionName("DeleteTargetGroup")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult DeleteTargetGroup([FromBody]int target_gp_id)
        {
            var _groupService = _ServiceFactory.GetService<ITargetGroupService>();
            _groupService.controller_key = _controller_key;
            var _updated = _groupService.Delete(target_gp_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }
        //
        [HttpGet]
        [ActionName("GetAllGranteesByTargetGP")]
        [ResponseType(typeof(List<granteeC>))]
        public IHttpActionResult GetAllGranteesByTargetGP([FromUri] int target_gp_id)
        {
            var _tg_Service = _ServiceFactory.GetService<IGranteeService>();
            _tg_Service.controller_key = _controller_key;
            var _list = _tg_Service.GetAllGranteesByTargetGroup(target_gp_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_list);

        }
        [HttpGet]
        [ActionName("GetAllUsersByTargetGP")]
        [ResponseType(typeof(List<dto_voice_userC>))]
        public IHttpActionResult GetAllUsersByTargetGP([FromUri] int target_gp_id, long fs_timestamp)
        {
            var _tg_Service = _ServiceFactory.GetService<IVoiceUserService>();
            _tg_Service.controller_key = _controller_key;
            var _list = _tg_Service.GetUsersByTargetGroup(target_gp_id, fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }
            return Ok(_list);

        }

        #endregion
        #region Grantee
        [HttpPost]
        [ActionName("AddGrantee")]
       
        [ResponseType(typeof(granteeC))]
        public IHttpActionResult AddGrantee(dto_grantee_newC _dto)
        {
            var _granteeService = _ServiceFactory.GetService<IGranteeService>();
            _granteeService.controller_key = _controller_key;
            var _grantee = _granteeService.AddNew(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_grantee);

        }
        //
        [HttpGet]
        [ActionName("GetAllGrantees")]
        
        [ResponseType(typeof(List<granteeC>))]
        public IHttpActionResult Grantees()
        {
            var _granteeService = _ServiceFactory.GetService<IGranteeService>();
            _granteeService.controller_key = _controller_key;
            var _grantee_list = _granteeService.GetAll().Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_grantee_list);

        }
        //
        [HttpGet]
        [ResponseType(typeof(List<granteeC>))]
        public IHttpActionResult GetAllGranteeWithFs([FromUri]long fs_timestamp)
        {
            var _granteeService = _ServiceFactory.GetService<IGranteeService>();
            _granteeService.controller_key = _controller_key;
            var _grantee_list = _granteeService.GetAll(fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_grantee_list);
        }
        //
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult EditGrantee(dto_grantee_update _dto)
        {
            var _granteeService = _ServiceFactory.GetService<IGranteeService>();
            _granteeService.controller_key = _controller_key;
            var _updated = _granteeService.Update(_dto).Result;
            if (!this.ModelState.IsValid)
            {

                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }
        //
        [HttpPost]
        [ActionName("DeleteGrantee")]
        public IHttpActionResult DeleteGrantee([FromBody] int id)
        {
            var _groupService = _ServiceFactory.GetService<IGranteeService>();
            _groupService.controller_key = _controller_key;
            var _updated = _groupService.Delete(id).Result;
            if (!this.ModelState.IsValid)
            {

                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }
        //
        [HttpGet]
         ResponseType(typeof(List<target_groupC>))]
        public IHttpActionResult GetAllTargetGPByGrantee([FromUri] int grantee_id)
        {
            var _tg_Service = _ServiceFactory.GetService<ITargetGroupService>();
            _tg_Service.controller_key = _controller_key;
            var _list = _tg_Service.GetAllTargetGroupsByGrantee(grantee_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }
            return Ok(_list);

        }
        #endregion
        #region Users
        [HttpPost]
        public IHttpActionResult AddUser(dto_voice_user_newC _dto)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _user = _userService.AddNew(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_user);

        }
        //
        [HttpPost]
        [ResponseType(typeof(dto_voice_userC))]
        public IHttpActionResult AddSubAdmin(dto_voice_sub_admin_newC _dto)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _user = _userService.AddNewSubAdmin(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_user);

        }



        [HttpGet]
        [ResponseType(typeof(List<dto_voice_userC>))]
        public IHttpActionResult Users()
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _gp_list = _userService.GetAll().Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_gp_list);

        }
        [HttpGet]
        [ResponseType(typeof(dto_UserPageRightsDataC))]
        public IHttpActionResult GetUserRightsTree([FromUri] int user_id)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _dto = _userService.GetUserRights(user_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_dto);

        }
        //
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult EditUser(dto_voice_user_updateC _dto)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _updated = _userService.Update(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_updated);

        }
        //
        [HttpPost]
       
        [ResponseType(typeof(bool))]
        public IHttpActionResult DeleteUser([FromBody]int user_id)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _updated = _userService.Delete(user_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());;
            }

            return Ok(_updated);

        }
        //
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult ChangeUserPassWord(dto_change_password _dto)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _updated = _userService.ChangeUserPwd(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_updated);

        }
        //
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult UpdatePcUserRights(dto_user_rights_updateC _dto)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _updated = _userService.UpdatePcUserRights(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_updated);

        }
        //
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult ChangeUserPcStatus(dto_user_rights_updateC _dto)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _updated = _userService.UpdatePcUserRights(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_updated);

        }
        //

        [HttpGet]
        [ResponseType(typeof(List<dto_voice_userC>))]
        public IHttpActionResult GetUsersByTargetGroup([FromUri] int target_group_id, long fs_timestamp)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _userList = _userService.GetUsersByTargetGroup(target_group_id, fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_userList);

        }
        //
        [HttpGet]
        [ResponseType(typeof(List<dto_voice_userC>))]
        public IHttpActionResult GetUsersByGrantee([FromUri]int grantee_id, long fs_timestamp)
        {
            var _userService = _ServiceFactory.GetService<IVoiceUserService>();
            _userService.controller_key = _controller_key;
            var _userList = _userService.GetUsersByGrantee(grantee_id, fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }

            return Ok(_userList);

        }

        #endregion
        #region resources
        [HttpPost]
        [ResponseType(typeof(voice_resourceC))]
        public IHttpActionResult AddResource(dto_voice_resource_new _dto)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _resource = _Service.AddResource(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_resource);
        }
        [HttpGet]
        [ResponseType(typeof(List<voice_resourceC>))]
        public IHttpActionResult GetAllResourcesByGrantee([FromUri]int grantee_id, long fs_timestamp)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllResourcesByGrantee(grantee_id, fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        [HttpGet]
        [ResponseType(typeof(List<voice_resourceC>))]
        public IHttpActionResult GetAllResourcesByGranteeResourceType([FromUri] int grantee_id, int resource_type_id, long fs_timestamp)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllResourcesByGrantee(grantee_id, resource_type_id, fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        //GetAllResourceCategoriesByTargetGroup
        [HttpGet]
        
        [ResponseType(typeof(List<voice_resource_categoryC>))]
        public IHttpActionResult GetAllResourceCategoriesByTargetGroup([FromUri] long fs_timestamp)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllResourceCategoriesByTargetGroup(fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }

        [HttpGet]
        
        [ResponseType(typeof(List<voice_resource_categoryC>))]
        public IHttpActionResult GetAllResourceCategoriesByGrantee([FromUri] long fs_timestamp)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllResourceCategoriesByGrantee(fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        [HttpGet]
        
        [ResponseType(typeof(List<voice_resource_categoryC>))]
        public IHttpActionResult GetAllResourceCategoriesByGrantType([FromUri] long fs_timestamp)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllResourceCategoriesByGrantType(fs_timestamp).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        //GetAllResourcesByCategory
        [HttpPost]
       
        [ResponseType(typeof(List<voice_resourceC>))]
        public IHttpActionResult GetAllResourcesByCategory(dto_getResourceByCategory _dto)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllResourcesByCategory(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }

        [HttpPost]
        [ActionName("RenameVoiceResource")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult RenameVoiceResource(dto_renameResourceC _dto)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.RenameResource(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        //
        [HttpPost]
        [ActionName("DeleteVoiceResource")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult DeleteVoiceResource([FromBody]int id)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.Delete(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        //
        [HttpPost]
        [ActionName("ResourceViewedNotification")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult ResourceViewedNotification([FromBody]int id)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.ResourceViewedNotification(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        //
        [HttpPost]
        [ActionName("ResourceDownloadedNotification")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult ResourceDownloadedNotification([FromBody]int id)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.ResourceDownloadedNotification(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        //DeleteResourceComment
        [HttpPost]
        [ActionName("DeleteResourceComment")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult DeleteResourceComment([FromBody]int id)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.DeleteResourceComment(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        [HttpPost]
        [ActionName("AddResourceComment")]
        [ResponseType(typeof(voice_resource_commentC))]
        public IHttpActionResult AddResourceCommente(dto_voice_resource_commentC _dto)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.AddResourceComment(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        [HttpGet]
        [ActionName("GetAllCommentsByResourceID")]
        [ResponseType(typeof(List<voice_resource_commentC>))]
        public IHttpActionResult GetAllCommentsByResourceID([FromUri] int resource_id)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetAllCommentsByResourceID(resource_id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        #endregion
        #region mydrive
        [HttpPost]
        [ActionName("AddResourceToMyDrive")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult AddResourceToMyDrive([FromBody]int id)
        {
            var _Service = _ServiceFactory.GetService<IMyDriveResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.AddResourceToMyDrive(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        [HttpGet]
        [ActionName("GetMyDriveResources")]
        [ResponseType(typeof(List<voice_resourceC>))]
        public IHttpActionResult GetMyDriveResources()
        {
            var _Service = _ServiceFactory.GetService<IMyDriveResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.GetAllMyDriveResourcesByUserId().Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }

        [HttpPost]
        [ActionName("RemoveResourceFromMyDrive")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult RemoveResourceFromMyDrive([FromBody]int id)
        {
            var _Service = _ServiceFactory.GetService<IMyDriveResourceService>();
            _Service.controller_key = _controller_key;
            var _result = _Service.RemoveResourceFromMyDrive(id).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_result);
        }
        #endregion
        #region resource_analysis
        [HttpPost]
        [ActionName("GetResourceAnalysisRangeGroupByUser")]
        [ResponseType(typeof(List<dto_analysis_r_user_statisticC>))]
        public IHttpActionResult GetResourceAnalysisRangeGroupByUser(dto_date_rangeRequestC _dto)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetResourceAnalysisUserByRange(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        //
        [HttpPost]
        [ActionName("GetResourceAnalysisRangeGranteeUploads")]
        [ResponseType(typeof(List<dto_analysis_r_grantee_upload_statisticC>))]
        public IHttpActionResult GetResourceAnalysisRangeGranteeUploads(dto_date_rangeRequestC _dto)
        {
            var _Service = _ServiceFactory.GetService<IResourceService>();
            _Service.controller_key = _controller_key;
            var _list = _Service.GetResourceAnalysisGranteeUploadsByRange(_dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState.ToErrorMessage());
            }
            return Ok(_list);
        }
        #endregion
    }
}
