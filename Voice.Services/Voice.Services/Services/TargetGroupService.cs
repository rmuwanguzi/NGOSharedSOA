
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;
using Voice.Service;
using Voice.Shared.Core;
using Voice.Shared.Core.Interfaces;
using System.Data.Common;
using System.Web.Hosting;
using System.IO;
using Dapper;

namespace Voice.Service
{
    public class TargetGroupService : ITargetGroupService
    {
        private string _table_name = "target_group_tb";
        public string controller_key { get; set; }
        private IMessageDialog _dialog;
        private void AddErrorMessage(string error_key, string _title, string _error_message)
        {
            if (!string.IsNullOrEmpty(controller_key))
            {
                _dialog.ErrorMessage(error_key, _error_message, controller_key);
            }
            else
            {
                _dialog.ErrorMessage(_error_message, "Save Error");
            }
        }
        public TargetGroupService(IMessageDialog dialog)
        {
            _dialog = dialog;
            _table_name = DbHelper.GetTableSchemaName(_table_name);
        }
        public Task<target_groupC> AddNew(dto_target_group_newC _dto)
        {
            target_groupC _obj = null;
            if (_dto == null)
            {
                AddErrorMessage("Insert Error", "Save Error", "Target Area Object Is Null");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (string.IsNullOrEmpty(_dto.target_gp_name))
            {
                AddErrorMessage("Insert Error", "Save Error", "Target Area Name Is Null");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_create_target_group))
            {
                _obj = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_obj);
            }
            #region _image_section

            string _imageUrl = null;
           
            if (!string.IsNullOrEmpty(_dto.target_gp_image_base_64))
            {
                string _imageFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.image_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Image File Extension Cannot Be Empty");
                    _obj = null;
                    return Task.FromResult(_obj);
                }
                try
                {
                    if(_dto.image_file_extension.IndexOf('.')==-1)
                    {
                        _file_extension = string.Format(".{0}", _dto.image_file_extension);
                    }
                    else
                    {
                        _file_extension = _dto.image_file_extension;
                    }
                    if(!fnn.IsValidImageFileExtension(_file_extension))
                    {
                        AddErrorMessage("Insert Error", "Save Error", "Image File Extension Is Invalid");
                        _obj = null;
                        return Task.FromResult(_obj);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.target_gp_image_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("tg-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                    _imageFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                    File.WriteAllBytes(_imageFilePath, _image_bytes);
                    _imageUrl = fnn.CreateImageAssetUrl(_file_name);
                }
                catch (Exception ex)
                {
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    _obj = null;
                    return Task.FromResult(_obj);
                }
            }
            #endregion
            using (var _db = fnn.GetDbContext())
            {
                _obj = new target_groupC();
                _obj.target_gp_name = _dto.target_gp_name.Trim().ToProperCase();
                _obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                _obj.server_edate = fnn.GetServerDate();
                _obj.created_by_user_id = fnn.LOGGED_USER.user_id;
                _obj.target_gp_image_url = _imageUrl;
                _obj.alias = _dto.alias;
                _db.TARGET_GROUPS.Add(_obj);
                var _retVal = _db.SaveChangesWithDuplicateKeyDetected();
                if (_retVal == null || _retVal.Value == true)
                {
                    AddErrorMessage("Duplicate Key Error", "Duplicate Key Error", "You Have Entered A Duplicate Target Group Name");
                    _obj = null;
                }
            }
            if (_obj != null)
            {
                using (var _dbb = fnn.GetDbContext())
                {
                    foreach (var k in Enum.GetValues(typeof(em.resource_typeS)))
                    {
                        var _robj = new voice_resource_categoryC();
                        _robj.created_by_user_id = fnn.LOGGED_USER.user_id;
                        _robj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                        _robj.resource_cat_id = _obj.target_gp_id;
                        _robj.resource_cat_img_url = _imageUrl;
                        _robj.resource_cat_name = _obj.target_gp_name.ToProperCase();
                        _robj.resource_type_id = k.ToInt16();
                        _robj.resource_cat_type_id = em.resource_cat_typeS.target_gp.ToInt32();
                        _robj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                        _robj.server_edate = fnn.GetServerDate();
                        _dbb.VOICE_RESOURCE_CATEGORIES.Add(_robj);
                        _dbb.SaveChanges();
                    }
                }
            }
            return Task.FromResult(_obj);
        }
        public Task<bool> Delete(int id)
        {
            bool _record_deleted = false;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_delete_target_group))
            {
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_record_deleted);
            }
            target_groupC _obj = null;
            using (var _db = fnn.GetDbContext())
            {
                 _obj = _db.TARGET_GROUPS.Where(e => e.target_gp_id == id & e.delete_id == 0).SingleOrDefault();
                if (_obj == null)
                {
                    _record_deleted = false;
                    AddErrorMessage("Delete Error", "Delete Error", "Could Not Find Target Area Object");
                }
                else
                {
                    var _has_dependency = DbHelper.HasDbDependencies(_db.Database, new string[] { "target_group_tb" }, DbHelper.GetDbSchema(), new string[] { "target_gp_id" }, id);
                    if (_has_dependency == null || _has_dependency == true)
                    {
                        AddErrorMessage("Delete Error", "Delete Error", "Unable To Delete Record Because It Has System Dependencies.");
                        _record_deleted = false;
                    }
                    else
                    {
                        var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                        {
                            pk_col_name = "target_gp_id",
                            pk_id = _obj.target_gp_id,
                            table_name = _table_name
                        }, _db);
                        if (_result == null || _result == false)
                        {
                            AddErrorMessage("Delete Error", "Delete Error", "Error Encountered While Trying To Delete Record");
                            _record_deleted = false;
                        }
                        else
                        {
                            _record_deleted = true;
                            _db.SaveChanges();
                        }
                    }

                }
                
            }
            if (_record_deleted)
            {
                Task.Factory.StartNew(() =>
                {
                    using (var _dbb = fnn.GetDbContext())
                    {
                        int _cat_type_id = em.resource_cat_typeS.target_gp.ToInt32();
                        var _r_list = (from k in _dbb.VOICE_RESOURCE_CATEGORIES
                                       where k.resource_cat_type_id == _cat_type_id & k.resource_cat_id == _obj.target_gp_id
                                       & k.delete_id == 0
                                       select k).ToList();
                        foreach (var _v in _r_list)
                        {
                            var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                            {
                                pk_col_name = _v.un_id.ToString(),
                                pk_id = _v.un_id,
                                table_name = DbHelper.GetTableSchemaName("voice_resource_cat_tb")
                            }, _dbb);
                            _dbb.SaveChanges();

                        }
                    }
                });
            }
            return Task.FromResult(_record_deleted);
        }
        public Task<List<target_groupC>> GetAll()
        {
            using (var _db = fnn.GetDbContext())
            {
               // _db.MapClassToTableName<target_groupC>(_table_name);
                var _list = _db.TARGET_GROUPS.Where(e => e.delete_id == 0).ToList();
                return Task.FromResult(_list);
            }
        }
        public Task<List<target_groupC>> GetAll(long fs_timestamp)
        {
            List<target_groupC> _list = null;
            using (var _db = fnn.GetDbContext())
            {
               // _db.MapClassToTableName<target_groupC>(_table_name);
                if (fs_timestamp == 0)
                {
                    _list = _db.TARGET_GROUPS.Where(e => e.delete_id == 0).ToList();
                }
                else
                {
                    _list = _db.TARGET_GROUPS.Where(e => e.fs_timestamp > fs_timestamp).ToList();
                }

            }
            return Task.FromResult(_list);
        }
        public Task<target_groupC> GetSingle(int id)
        {
            target_groupC  _obj = null;
            using (var _db = fnn.GetDbContext())
            {
               // _db.MapClassToTableName<target_groupC>(_table_name);
                _obj = _db.TARGET_GROUPS.Where(e => e.target_gp_id == id & e.delete_id == 0).FirstOrDefault();
                if (_obj == null)
                {
                    AddErrorMessage("Search Error", "Error", "Unable to Find Record");
                    _obj = null;
                }
                return Task.FromResult(_obj);
            }
        }
        public Task<bool> Update(dto_target_group_updateC _dto)
        {
            bool _updated = false;
            if (string.IsNullOrEmpty(_dto.target_gp_name))
            {
                AddErrorMessage("Update Error", "Save Error", "Target Area Name Is Null!");
                return Task.FromResult(_updated);
            }
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_target_group_info))
            {
                _updated = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_updated);
            }
            #region _image_section

            string _imageUrl = null;
            target_groupC _obj = null;
            if (!string.IsNullOrEmpty(_dto.target_gp_image_base_64))
            {
                string _imageFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.image_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Grantee Image File Extension Cannot Be Empty");
                    return Task.FromResult(_updated);
                }
                try
                {
                    if (_dto.image_file_extension.IndexOf('.') == -1)
                    {
                        _file_extension = string.Format(".{0}", _dto.image_file_extension);
                    }
                    else
                    {
                        _file_extension = _dto.image_file_extension;
                    }
                    if (!fnn.IsValidImageFileExtension(_file_extension))
                    {
                        AddErrorMessage("Insert Error", "Save Error", "Grantee Image File Extension Is Invalid");
                        return Task.FromResult(_updated);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.target_gp_image_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Grantee Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("tg-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                    _imageFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                    File.WriteAllBytes(_imageFilePath, _image_bytes);
                    _imageUrl = fnn.CreateImageAssetUrl(_file_name);
                }
                catch (Exception ex)
                {
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    return Task.FromResult(_updated);
                }
            }
            #endregion
            using (var _trans = new ZUpdateContext())
            {
                var _existing = _trans.Context.TARGET_GROUPS.Where(e => e.target_gp_id == _dto.target_gp_id & e.delete_id == 0).FirstOrDefault();
                if (_existing == null)
                {
                    _updated = false;
                    AddErrorMessage("Update Error", "Save Error", "Unable To Find Facilitator Object");
                    return Task.FromResult(_updated);
                }
                _obj = _existing;
                if (_existing.target_gp_name.ToLower() != _dto.target_gp_name.ToLower())
                {
                    var _ret = DbHelper.UpdatePrimaryKeyColumn(new DbHelperPrimarykeyUpdateC
                    {
                        col_to_update = "target_gp_name",
                        new_col_value = _dto.target_gp_name.Trim().ToProperCase(),
                        table_name = _table_name,
                        pk_col_name = "target_gp_id",
                        pk_id = _dto.target_gp_id
                    }, _trans.Context);
                    if (_ret == null || _ret.Value == false)
                    {
                        AddErrorMessage("Error", "Update Error", "Target Area Name Already Exists");
                        _updated = false;
                        return Task.FromResult(_updated);
                    }
                    else
                    {
                        _updated = true;
                        _trans.Context.SaveChanges();
                    }
                }
                _updated = true;
                if (_updated)
                {
                    List<DbParameter> _para = new List<DbParameter>();
                    _para.Clear();
                    _para.Add(fnn.GetDbParameters("@id", _existing.target_gp_id));
                    _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                    _para.Add(fnn.GetDbParameters("@v3", string.IsNullOrEmpty(_dto.alias) ? string.Empty : _dto.alias));
                    _para.Add(fnn.GetDbParameters("@v4", string.IsNullOrEmpty(_imageUrl) ? string.Empty : _imageUrl));
                    //
                    var _sql = string.Format(string.Format("update {0} set fs_timestamp=@v2,alias=@v3,target_gp_image_url=@v4 where target_gp_id = @id and delete_id = 0", _table_name));
                    _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                    _trans.Context.SaveChanges();

                    int _cat_type_id = em.resource_cat_typeS.target_gp.ToInt32();
                    var _r_list = (from k in _trans.Context.VOICE_RESOURCE_CATEGORIES
                                   where k.resource_cat_type_id == _cat_type_id & k.resource_cat_id == _obj.target_gp_id
                                   & k.delete_id == 0
                                   select k).ToList();
                    foreach (var _v in _r_list)
                    {
                        _para.Clear();
                        _para.Add(fnn.GetDbParameters("@v1", _existing.target_gp_id));
                        _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                        _para.Add(fnn.GetDbParameters("@v3", string.IsNullOrEmpty(_imageUrl) ? string.Empty : _imageUrl));
                        _para.Add(fnn.GetDbParameters("@v4", _dto.target_gp_name.Trim().ToProperCase()));
                        _sql = string.Format(string.Format("update {0} set fs_timestamp=@v2,resource_cat_img_url=@v3" +
                          ",resource_cat_name=@v4 where un_id = @v1 and delete_id = 0", DbHelper.GetTableSchemaName("voice_resource_cat_tb")));
                        _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                        _trans.Context.SaveChanges();
                    }

                    _trans.Commit();
                }
            }
            return Task.FromResult(_updated);
        }
        public Task<List<target_groupC>> GetAllTargetGroupGrantType(int grant_type_id)
        {
            List<target_groupC> _tg_List = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_target_group_page))
            {
                _tg_List = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_tg_List);
            }
            var _sql = string.Format("select m.* from {0} as m, {1}  as c" +
                " where c.grant_type_id= {2} and c.target_gp_id = m.target_gp_id and m.delete_id = 0 ",
                _table_name,
                DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                grant_type_id);
            using (var _db = fnn.GetDbConnection())
            {
                _tg_List = _db.Query<target_groupC>(_sql).ToList();
            }
            return Task.FromResult(_tg_List);
        }

        public Task<List<target_groupC>> GetAllTargetGroupsByGrantee(int grantee_id)
        {
            List<target_groupC> _tg_List = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_target_group_page))
            {
                _tg_List = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_tg_List);
            }
            var _sql = string.Format("select m.* from {0} as m, {1}  as c" +
                " where c.grantee_id= {2} and c.target_gp_id = m.target_gp_id and m.delete_id = 0 ",
                _table_name,
                DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                grantee_id);
            using (var _db = fnn.GetDbConnection())
            {
                _tg_List = _db.Query<target_groupC>(_sql).ToList();
            }
            return Task.FromResult(_tg_List);
        }
    }
}
