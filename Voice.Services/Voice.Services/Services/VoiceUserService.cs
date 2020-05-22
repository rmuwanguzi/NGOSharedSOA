using Voice.Shared.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Models;
using Voice.Service;
using Voice.Shared.Core;
using System.Data.Common;
using System.Web.Hosting;
using System.IO;
using System.Reflection;
using Dapper;

namespace Voice.Service
{
    public class VoiceUserService : IVoiceUserService
    {
        public string controller_key { get; set; }
        private IMessageDialog _dialog;
        private string _table_name = "voice_user_tb";
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
        public VoiceUserService(IMessageDialog dialog)
        {
            _dialog = dialog;
            _table_name = DbHelper.GetTableSchemaName(_table_name);
        }
        public Task<dto_voice_userC> AddNew(dto_voice_user_newC _dto)
        {
           
            dto_voice_userC _obj_ret = null;
            voice_userC _obj = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_create_new_user))
            {
                _obj_ret = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_obj_ret);
            }
            if (_dto == null)
            {
                AddErrorMessage("Insert Error", "Save Error", "Voice User Object Is Null");
                _obj_ret = null;
                return Task.FromResult(_obj_ret);
            }
            if (string.IsNullOrEmpty(_dto.user_name))
            {
                AddErrorMessage("Insert Error", "Save Error", "User Name Is Null");
                _obj_ret = null;
                return Task.FromResult(_obj_ret);
            }
            if(string.IsNullOrEmpty(_dto.email_address))
            {
                AddErrorMessage("Insert Error", "Save Error", "Email Address is Null");
                return Task.FromResult(_obj_ret);
            }
            #region _image_section
            string _imageUrl = null;
            _obj_ret = null;
            if (!string.IsNullOrEmpty(_dto.user_image_base_64))
            {
                string _imageFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.image_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Image File Extension Cannot Be Empty");
                    return Task.FromResult(_obj_ret);
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
                        AddErrorMessage("Insert Error", "Save Error", "Image File Extension Is Invalid");
                        _obj = null;
                        return Task.FromResult(_obj_ret);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.user_image_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        string _file_name = string.Format("user-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                        _imageFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                        File.WriteAllBytes(_imageFilePath, _image_bytes);
                        _imageUrl = fnn.CreateImageAssetUrl(_file_name);
                    }
                    else
                    { 
}
                }
                catch (Exception ex)
                {
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    return Task.FromResult(_obj_ret);
                }
            }
            #endregion
            granteeC _grantee = null;
            grant_typeC _grant_type = null;
            using (var _db = fnn.GetDbContext())
            {

                _grantee = _db.GRANTEES.FirstOrDefault(g => g.grantee_id == _dto.grantee_id & g.delete_id == 0);
                if (_grantee==null)
                {
                    AddErrorMessage("Insert Error", "Save Error", "Grantee Object Not Found");
                    _obj_ret = null;
                    return Task.FromResult(_obj_ret);
                }
                //
               
                _grant_type = _db.GRANT_TYPES.FirstOrDefault(g => g.grant_type_id == _grantee.grant_type_id & g.delete_id == 0);
                //
                _obj = new voice_userC();
                _obj.user_name = _dto.user_name.Trim().ToProperCase();
                _obj.phone_no = _dto.phone_no.Trim();
                _obj.grant_type_name = _grant_type.grant_type_name;
                _obj.grantee_name = _grantee.grantee_name;
                _obj.email_address = _dto.email_address.Trim();
                _obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                _obj.server_edate = fnn.GetServerDate();
                _obj.grant_type_id = _grantee.grant_type_id;
                _obj.user_status_id = em.user_statuS.enabled.ToInt16();
                _obj.user_type_id = em.user_typeS.grantee.ToInt16();
                _obj.grantee_id = _dto.grantee_id;
                _obj.user_login_id = _dto.email_address;
                _obj.user_login_access_id = em.user_login_accessS.granted.ToInt16();
                _obj.user_login_pwd = fnn.sha256_hash("1234");
                switch(fnn.LOGGED_USER.user_level)
                {
                    case em.user_levelS.facilitator:
                    case em.user_levelS.sub_admin:
                        {
                            _obj.user_level_id = em.user_levelS.grantee_admin.ToInt16();
                            break;
                        }
                    default:
                        {
                            _obj.user_level_id = em.user_levelS.grantee_user.ToInt16();
                            break;
                        }
                }
                
                _obj.created_by_user_id = fnn.LOGGED_USER.user_id;
                _obj.user_picture_url = _imageUrl;
                _db.VOICE_USERS.Add(_obj);
                var _retVal = _db.SaveChangesWithDuplicateKeyDetected();
                if (_retVal == null || _retVal.Value == true)
                {
                    AddErrorMessage("Duplicate Key Error", "Duplicate Key Error", "You Have Entered A Duplicate Grantee User Name");
                    _obj_ret = null;
                }

            }
            _obj_ret = new dto_voice_userC();
            _obj_ret.grant_type_name = _obj.grant_type_name;
            _obj_ret.grant_type_id = _obj.grant_type_id;
            _obj_ret.phone_no = _obj.phone_no;
            _obj_ret.user_id = _obj.user_id;
            _obj_ret.user_login_access_id = _obj.user_login_access_id;
            _obj_ret.user_name = _obj.user_name;
            _obj_ret.user_rights_ids_string = _obj.user_rights_ids_string;
            _obj_ret.user_status_id = _obj.user_status_id;
            _obj_ret.user_type_id = _obj.user_type_id;
            _obj_ret.email_address = _obj.email_address;
            _obj_ret.user_picture_url = _imageUrl;
                       
            Task.Factory.StartNew(() =>
            {
                if (_obj != null)
                {
                    Caching.CacheManager.ClearCache(Caching.em.cache_keys.grantee);
                    string _sql = null;
                    int _count = 0;
                    List<DbParameter> _para = new List<DbParameter>();
                    Dictionary<string, string> _dictionary = new Dictionary<string, string>();
                    using (var _dbb = fnn.GetDbContext())
                    {
                        try
                        {
                            _dictionary.Add("user_id", "no_of_users");
                            foreach (var _v in _dictionary)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grant_type_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grant_type_id=@v1 and delete_id=0", _v.Key, _table_name);
                                _count = _dbb.Database.ExecuteScalar(_sql,_para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grant_type_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grant_type_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("grant_type_tb"), _v.Value, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                            }
                                                 
                            //update grantee_tb
                            _dictionary.Clear();
                            _dictionary.Add("user_id", "no_of_users");
                            foreach (var _v in _dictionary)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grantee_id=@v1 and delete_id=0", _v.Key, _table_name);
                                _count = _dbb.Database.ExecuteScalar(_sql,_para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grantee_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grantee_id = @id and delete_id = 0", "grantee_tb".ToDbSchemaTable(), _v.Value, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();

                            }
                            int _tg_id = _dbb.Database.ExecuteScalar(string.Format("select TOP 1 target_gp_id from {0} where grantee_id={1} and delete_id=0", "assign_grantee_to_target_group".ToDbSchemaTable(), _obj.grantee_id)).ToInt32();
                            if (_tg_id > 0)
                            {
                                var _gt_ids = _dbb.ASSIGN_GRANTEE_TO_TARGET_GROUP.Where(h => h.target_gp_id == _tg_id & h.delete_id==0).Select(r => r.grantee_id).ToList();
                                if (_gt_ids.Count > 0)
                                {
                                    var _in_string = string.Join(",", _gt_ids);
                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                                    _sql = string.Format("SELECT COUNT(DISTINCT user_id) from {0} where grantee_id IN ({1}) and delete_id=0", _table_name, _in_string);
                                    _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();

                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@id", _tg_id));
                                    _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                    _para.Add(fnn.GetDbParameters("@v3", _count));
                                    _sql = string.Format(string.Format("update {0} set no_of_users = @v3,fs_timestamp=@v2 where target_gp_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("target_group_tb")));
                                    _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                    _dbb.SaveChanges();
                                }
                            }
                            
                        }
                        catch (DbException ex)
                        {
                            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                        
                    }
                }

            });
            return Task.FromResult(_obj_ret);
        }
        public Task<List<dto_voice_userC>> GetAll()
        {
            string _sql = null;
            _sql = string.Format("select * from {0} where delete_id=0", _table_name);
            List <dto_voice_userC> _ret_List = null;
            using (var _db = fnn.GetDbConnection())
            {
                _ret_List = _db.Query<voice_userC>(_sql).Select(g => new dto_voice_userC
                {
                    email_address = g.email_address,
                    grantee_id = g.grantee_id,
                    access_token = string.Empty,
                    grantee_name = g.grantee_name,
                    grant_type_id = g.grant_type_id,
                    grant_type_name = g.grant_type_name,
                    phone_no = g.phone_no,
                    user_id = g.user_id,
                    user_login_access_id = g.user_login_access_id,
                    user_login_id = g.user_login_id,
                    user_name = g.user_name,
                    user_rights_ids_string = g.user_rights_ids_string,
                    user_status_id = g.user_status_id,
                    user_type_id = g.user_type_id,
                    user_picture_url = g.user_picture_url,
                    user_level_id=g.user_level_id
                }).ToList();

            }
            return Task.FromResult(_ret_List);
        }
        public Task<List<dto_voice_userC>> GetAll(long fs_timestamp)
        {
            string _sql = null;
            List<dto_voice_userC> _ret_List = null;
            _sql = string.Format("select * from {0} where delete_id=0", _table_name);
            if (fs_timestamp > 0)
            {
                _sql = string.Format("select * from {0} where fs_timestamp > {1}", _table_name, fs_timestamp);
            }
            using (var _db = fnn.GetDbConnection())
            {
                _ret_List = _db.Query<voice_userC>(_sql).Select(g => new dto_voice_userC
                {
                    email_address = g.email_address,
                    grantee_id = g.grantee_id,
                    access_token = string.Empty,
                    grantee_name = g.grantee_name,
                    grant_type_id = g.grant_type_id,
                    grant_type_name = g.grant_type_name,
                    phone_no = g.phone_no,
                    user_id = g.user_id,
                    user_login_access_id = g.user_login_access_id,
                    user_login_id = g.user_login_id,
                    user_name = g.user_name,
                    user_rights_ids_string = g.user_rights_ids_string,
                    user_status_id = g.user_status_id,
                    user_type_id = g.user_type_id,
                    user_picture_url = g.user_picture_url,
                     user_level_id = g.user_level_id
                }).ToList();
              
            }
            return Task.FromResult(_ret_List);

        }
        public Task<dto_voice_userC> GetSingle(int id)
        {
            string _sql = null;
            dto_voice_userC _ret_obj = null;
            _sql = string.Format("select * from {0} where user_id={1} and delete_id=0", _table_name, id);
            using (var _db = fnn.GetDbConnection())
            {
                _ret_obj = _db.Query<voice_userC>(_sql).Select(g => new dto_voice_userC
                {
                    email_address = g.email_address,
                    grantee_id = g.grantee_id,
                    access_token = string.Empty,
                    grantee_name = g.grantee_name,
                    grant_type_id = g.grant_type_id,
                    grant_type_name = g.grant_type_name,
                    phone_no = g.phone_no,
                    user_id = g.user_id,
                    user_login_access_id = g.user_login_access_id,
                    user_login_id = g.user_login_id,
                    user_name = g.user_name,
                    user_rights_ids_string = g.user_rights_ids_string,
                    user_status_id = g.user_status_id,
                    user_type_id = g.user_type_id,
                    user_picture_url=g.user_picture_url,
                    user_level_id = g.user_level_id
                }).FirstOrDefault();
             
            }
            return Task.FromResult(_ret_obj);

        }
        public Task<bool> Update(dto_voice_user_updateC _dto)
        {
            bool _updated = false;
            if (string.IsNullOrEmpty(_dto.user_name))
            {
                AddErrorMessage("Update Error", "Save Error", "User Name Is Null!");
                return Task.FromResult(_updated);
            }
            if(string.IsNullOrEmpty(_dto.user_image_base64))
            {
                AddErrorMessage("Update Error", "Save Error", "No Image");
                return Task.FromResult(_updated);
            }
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_user_info))
            {
                _updated = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_updated);
            }
            #region _image_section
            string _imageUrl = null;
            if (!string.IsNullOrEmpty(_dto.user_image_base64))
            {
                string _imageFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.image_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Image File Extension Cannot Be Empty");
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
                        AddErrorMessage("Insert Error", "Save Error", "Image File Extension Is Invalid");
                        return Task.FromResult(_updated);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.user_image_base64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("user-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
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
            try
            {
                using (var _trans = new ZUpdateContext())
                {
                    var _existing = _trans.Context.VOICE_USERS.Where(e => e.user_id == _dto.user_id & e.delete_id == 0).FirstOrDefault();
                    if (_existing == null)
                    {
                        _updated = false;
                        AddErrorMessage("Update Error", "Save Error", "Unable To Find User Object");
                        return Task.FromResult(_updated);
                    }
                    if (_existing.user_type == em.user_typeS.guest_user)
                    {
                        _updated = false;
                        AddErrorMessage("Update Error", "Save Error", "You Cannot Edit Guest Account Info");
                        return Task.FromResult(_updated);
                    }

                    if (_existing.user_name.ToLower() != _dto.user_name.ToLower())
                    {
                        var _ret = DbHelper.UpdatePrimaryKeyColumn(new DbHelperPrimarykeyUpdateC
                        {
                            col_to_update = "email_address",
                            new_col_value = _dto.email_address.Trim().ToProperCase(),
                            table_name = _table_name,
                            pk_col_name = "user_id",
                            pk_id = _dto.user_id
                        }, _trans.Context);
                        if (_ret == null || _ret.Value == false)
                        {
                            AddErrorMessage("Error", "Update Error", "Email Address Already Exists");
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
                    List<DbParameter> _para = new List<DbParameter>();
                    _para.Clear();
                    _para.Add(fnn.GetDbParameters("@v1", _existing.user_id));
                    _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                    _para.Add(fnn.GetDbParameters("@v3", string.IsNullOrEmpty(_dto.email_address) ? string.Empty : _dto.email_address));
                    _para.Add(fnn.GetDbParameters("@v4", string.IsNullOrEmpty(_imageUrl) ? string.Empty : _imageUrl));
                    _para.Add(fnn.GetDbParameters("@v5", string.IsNullOrEmpty(_dto.phone_no) ? string.Empty : _dto.phone_no));
                    //
                    var _sql = string.Format(string.Format("update {0} set fs_timestamp=@v2,email_address=@v3,user_picture_url=@v4" +
                        ",phone_no=@v5 " +
                        "where user_id = @v1 and delete_id = 0", _table_name));
                    _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                    _trans.Context.SaveChanges();
                    _trans.Commit();

                }
            }
            catch(DbException ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _updated = false;
            }
            catch(Exception ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _updated = false;
            }
            return Task.FromResult(_updated);
           
        }
        public Task<bool> UpdatePcUserRights(dto_user_rights_updateC _dto)
        {
            bool _update_status = true;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_user_rights))
            {
                _update_status = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_update_status);
            }
           
            using (var _db = fnn.GetDbContext())
            {
                var _user = (from k in _db.VOICE_USERS
                             where k.user_id == _dto.user_id & k.delete_id==0
                             select k).FirstOrDefault();
                if (_user == null)
                {
                    AddErrorMessage("Error", "Error", "No Associated User Found");
                    _update_status = false;
                }
                _user.user_rights_ids_string = _dto.rights_string;
                _db.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            return Task.FromResult(_update_status);
        }
        public Task<bool> ChangeUserPwd(dto_change_password _dto)
        {
            bool _change_status = false;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_password))
            {
                _change_status = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_change_status);
            }
            if (_dto == null)
            {
                AddErrorMessage("Error", "Error", "Object Is Null");
                return Task.FromResult(_change_status);
            }
            using (var _db = fnn.GetDbContext())
            {
                var _user = (from k in _db.VOICE_USERS
                             where k.user_id == _dto.user_id & k.delete_id == 0
                             select k).FirstOrDefault();
                if (_user == null)
                {
                    AddErrorMessage("Update Error", "Error", "No Associated User Found");
                    _change_status = false;
                    return Task.FromResult(_change_status);
                }
                if (_dto.old_pwd != _user.user_login_pwd)
                {
                    AddErrorMessage("Update Error", "Error", "You Have Entered A Wrong OLD PASSWORD");
                    _change_status = false;
                    return Task.FromResult(_change_status);
                }
                if (string.IsNullOrWhiteSpace(_dto.new_pwd))
                {
                    AddErrorMessage("Update Error", "Error", "You Have Entered An Empty NEW PASSWORD");
                    _change_status = false;
                    return Task.FromResult(_change_status);
                }
                List<DbParameter> _para = new List<DbParameter>();
                _para.Clear();
                _para.Add(fnn.GetDbParameters("@v1", _dto.new_pwd));
                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                _para.Add(fnn.GetDbParameters("@v3", _dto.user_id));
                string _sql = string.Format("update {0} set user_login_pwd=@v1,fs_timestamp=@v2 where user_id=@v3 and delete_id=0", _table_name);
                var _ret_id = _db.Database.ExecuteSqlCommand(_sql, _para.ToArray());

                if (_ret_id == 0)
                {
                    AddErrorMessage("Update Error", "Database Error", "Failed To Update Password Due To Database Error");
                    _change_status = false;
                    return Task.FromResult(_change_status);
                }
                else
                {
                    _change_status = true;
                    _db.SaveChanges();
                }


            }
            return Task.FromResult(_change_status);
        }
        public Task<bool> Delete(int id)
        {
            bool _record_deleted = false;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_delete_user))
            {
                _record_deleted = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_record_deleted);
            }
            voice_userC _obj = null;
            using (var _db = fnn.GetDbContext())
            {
                _obj = _db.VOICE_USERS.Where(e => e.user_id == id & e.delete_id == 0).SingleOrDefault();
                if (_obj == null)
                {
                    _record_deleted = false;
                    AddErrorMessage("Delete Error", "Delete Error", "Could Not Find User Object");
                    return Task.FromResult(_record_deleted);
                }
                else
                {
                    if (_obj.user_type == em.user_typeS.facilitator)
                    {
                        _record_deleted = false;
                        AddErrorMessage("Delete Error", "Delete Error", "You Cannot Delete A Facilitator");
                        return Task.FromResult(_record_deleted);
                    }
                    if (_obj.user_type == em.user_typeS.guest_user)
                    {
                        _record_deleted = false;
                        AddErrorMessage("Delete Error", "Delete Error", "You Cannot Delete A Guest Account");
                        return Task.FromResult(_record_deleted);
                    }
                    if (fnn.LOGGED_USER.user_id == _obj.user_id)
                    {
                        _record_deleted = false;
                        AddErrorMessage("Delete Error", "Delete Error", "The Logged In User Can Only Be Deleted By Another Superior User");
                        return Task.FromResult(_record_deleted);
                    }
                    
                    var _has_dependency = DbHelper.HasDbDependencies(_db.Database, new string[] { "voice_user_tb" }, DbHelper.GetDbSchema(), new string[] { "user_id", "delete_pc_us_id", "created_by_user_id" }, id);
                    if (_has_dependency == null || _has_dependency == true)
                    {
                        AddErrorMessage("Delete Error", "Delete Error", "Unable To Delete Record Because It Has System Dependencies.");
                        _record_deleted = false;
                    }
                    else
                    {
                        var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                        {
                            pk_col_name = "user_id",
                            pk_id = _obj.user_id,
                            table_name = _table_name
                        }, _db);
                        _record_deleted = true;
                    }
                }
            }
            Task.Factory.StartNew(() =>
            {
                if (_record_deleted)
                {
                    Caching.CacheManager.ClearCache(Caching.em.cache_keys.grantee);
                    string _sql = null;
                    int _count = 0;
                    List<DbParameter> _para = new List<DbParameter>();
                    Dictionary<string, string> _dictionary = new Dictionary<string, string>();
                    using (var _dbb = fnn.GetDbContext())
                    {
                        try
                        {
                                               
                            _dictionary.Add("user_id", "no_of_users");
                            foreach (var _v in _dictionary)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grant_type_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grant_type_id=@v1 and delete_id=0", _v.Key, _table_name);
                                _count = _dbb.Database.ExecuteScalar(_sql,_para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grant_type_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grant_type_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("grant_type_tb"), _v.Value, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                            }
                            
                            //update grantee_tb
                            _dictionary.Clear();
                            _dictionary.Add("user_id", "no_of_users");
                            foreach (var _v in _dictionary)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grantee_id=@v1 and delete_id=0", _v.Key, _table_name);
                                _count = _dbb.Database.ExecuteScalar(_sql,_para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grantee_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grantee_id = @id and delete_id = 0", "grantee_tb".ToDbSchemaTable(), _v.Value, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                               _dbb.SaveChanges();


                            }
                            int _tg_id = _dbb.Database.ExecuteScalar(string.Format("select TOP 1 target_gp_id from {0} where grantee_id={1} and delete_id=0", "assign_grantee_to_target_group".ToDbSchemaTable(), _obj.grantee_id)).ToInt32();
                            if (_tg_id > 0)
                            {
                                var _gt_ids = _dbb.ASSIGN_GRANTEE_TO_TARGET_GROUP.Where(h => h.target_gp_id == _tg_id & h.delete_id == 0).Select(r => r.grantee_id).ToList();
                                if (_gt_ids.Count > 0)
                                {
                                    var _in_string = string.Join(",", _gt_ids);
                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                                    _sql = string.Format("SELECT COUNT(DISTINCT user_id) from {0} where grantee_id IN ({1}) and delete_id=0", _table_name, _in_string);
                                    _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();

                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@id", _tg_id));
                                    _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                    _para.Add(fnn.GetDbParameters("@v3", _count));
                                    _sql = string.Format(string.Format("update {0} set no_of_users = @v3,fs_timestamp=@v2 where target_gp_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("target_group_tb")));
                                    _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                    _dbb.SaveChanges();
                                }
                            }


                        }
                        catch (DbException ex)
                        {
                            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }

                    }
                }

            });                
                return Task.FromResult(_record_deleted);
            
        }
        public Task<bool> ChangeUserPcStatus(dto_user_login_access_updateC _dto)
        {
            bool _record_updated = false;
            if (_dto == null)
            {
                _record_updated = false;
                AddErrorMessage("Update Error", "Update Error", "Change User Status Object Is Null");
                return Task.FromResult(_record_updated);
            }
            //if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_))
            //{
            //    _update_status = false;
            //    AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
            //    return Task.FromResult(_update_status);
            //}
            using (var _db = fnn.GetDbContext())
            {
                var old_staff_status = (em.user_statuS)_dto.old_status;
                var _user = _db.VOICE_USERS.Where(e => e.user_id == _dto.user_id & e.delete_id == 0 & e.user_status_id  == _dto.old_status).SingleOrDefault();
                if (_user == null)
                {
                    _record_updated = false;
                    AddErrorMessage("Update Error", "Update Error", "Associated User Object Can Not Be Found.");
                    return Task.FromResult(_record_updated);
                }
                else
                {
                    _user.user_status_id = _dto.new_status;
                    if (_user.user_status == em.user_statuS.disabled)
                    {
                        _user.user_login_access_id  = em.user_login_accessS.denied.ToInt16();
                    }
                    _user.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                    _db.VOICE_USERS.AddOrUpdateExtension(_user);
                    _db.SaveChanges();
                    _record_updated = true;
                    return Task.FromResult(_record_updated);
                }
            }
        }
        public Task<List<dto_voice_userC>> GetUsersByTargetGroup(int target_group_id, long fs_timestamp)
        {
            string _sql = null;
            List<dto_voice_userC> _ret_List = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_user_page))
            {
                _ret_List = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_ret_List);
            }
            if (fs_timestamp > 0)
            {
                _sql = string.Format("select m.* from {0} as m ," +
                    "{1} as k where k.target_gp_id = {2} and m.grantee_id = k.grantee_id and" +
                    " m.fs_timestamp > {3} ",
                    _table_name,
                    DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                    target_group_id, fs_timestamp);
            }
            else
            {
                _sql = string.Format("select m.* from {0} as m ," +
                    "{1} as k where k.target_gp_id = {2} and m.grantee_id = k.grantee_id and" +
                    " m.delete_id=0",
                    _table_name,
                    DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                    target_group_id);
            }
            using (var _db = fnn.GetDbConnection())
            {
                _ret_List = _db.Query<voice_userC>(_sql).Select(g => new dto_voice_userC
                {
                    email_address = g.email_address,
                    grantee_id = g.grantee_id,
                    access_token = string.Empty,
                    grantee_name = g.grantee_name,
                    grant_type_id = g.grant_type_id,
                    grant_type_name = g.grant_type_name,
                    phone_no = g.phone_no,
                    user_id = g.user_id,
                    user_login_access_id = g.user_login_access_id,
                    user_login_id = g.user_login_id,
                    user_name = g.user_name,
                    user_rights_ids_string = g.user_rights_ids_string,
                    user_status_id = g.user_status_id,
                    user_type_id = g.user_type_id,
                    user_picture_url = g.user_picture_url,
                    user_level_id = g.user_level_id

                }).ToList();
                
            }
            return Task.FromResult(_ret_List);
        }
        public Task<List<dto_voice_userC>> GetUsersByGrantee(int grantee_id, long fs_timestamp)
        {
            string _sql = null;
            List<dto_voice_userC> _ret_List = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_user_page))
            {
                _ret_List = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_ret_List);
            }
            if (fs_timestamp > 0)
            {
                _sql = string.Format("select * from {0}  where grantee_id={1} and delete_id=0 and fs_timestamp > {2} ", _table_name, grantee_id, fs_timestamp);
            }
            else
            {
                _sql = string.Format("select * from {0}  where grantee_id={1} and delete_id=0", _table_name, grantee_id);
            }
            using (var _db = fnn.GetDbConnection())
            {
                _ret_List = _db.Query<voice_userC>(_sql).Select(g => new dto_voice_userC
                {
                    email_address = g.email_address,
                    grantee_id = g.grantee_id,
                    access_token = string.Empty,
                    grantee_name = g.grantee_name,
                    grant_type_id = g.grant_type_id,
                    grant_type_name = g.grant_type_name,
                    phone_no = g.phone_no,
                    user_id = g.user_id,
                    user_login_access_id = g.user_login_access_id,
                    user_login_id = g.user_login_id,
                    user_name = g.user_name,
                    user_rights_ids_string = g.user_rights_ids_string,
                    user_status_id = g.user_status_id,
                    user_type_id = g.user_type_id,
                    user_picture_url = g.user_picture_url,
                    user_level_id = g.user_level_id
                }).ToList();
               
               
                
            }
            return Task.FromResult(_ret_List);
        }
        public Task<dto_UserPageRightsDataC> GetUserRights(int user_id)
        {
            voice_userC _user = null;
            dto_UserPageRightsDataC _dto = null;
            using (var _db = fnn.GetDbContext())
            {
              //  _db.MapClassToTableName<voice_userC>("voice_user_tb");
                _user = _db.VOICE_USERS.Where(h => h.user_id == user_id).FirstOrDefault();
            }
            if (_user == null)
            {
                _dto = null;
                AddErrorMessage("Update Error", "Update Error", "User Object Not Found");
                return Task.FromResult(_dto);
            }
            if (_user.user_type == em.user_typeS.facilitator)
            {
                _dto = null;
                AddErrorMessage("Update Error", "Update Error", "You Cannot Update Rights Of A Facilitator");
                return Task.FromResult(_dto);
            }
            if (_user.user_type == em.user_typeS.guest_user)
            {
                if (fnn.LOGGED_USER.user_level != em.user_levelS.facilitator)
                {
                    _dto = null;
                    AddErrorMessage("Update Error", "Update Error", "You Not Authorized To Update the Rights Of A Guest Account");
                    return Task.FromResult(_dto);
                }
            }
            if (_user.delete_id>0)
            {
                AddErrorMessage("Update Error", "Update Error", "This User Has Already Been Deleted");
                return Task.FromResult(_dto);
            }
            string _json_string = string.Empty;
            Assembly m_assembly;
            m_assembly = Assembly.GetExecutingAssembly();
            Stream l_stream = null;
            l_stream = m_assembly.GetManifestResourceStream("Voice.Service._voice_rights_.json");//add the namespace use embedded resource
            if(l_stream==null)
            {
                AddErrorMessage("Fetch Error", "Fetch Error", "Failed To Get Rights JSON Data Stream");
                return Task.FromResult(_dto);
            }
            using (StreamReader streamReader = new StreamReader(l_stream, Encoding.UTF8))
            {
                _json_string = streamReader.ReadToEnd();
            }
            var _pageRights = Newtonsoft.Json.JsonConvert.DeserializeObject<List<pageC>>(_json_string);
            _dto = new dto_UserPageRightsDataC();
            _dto.UserPageRightsTree = _pageRights;
            _dto.user_id = _user.user_id;
            _dto.user_rights_ids = _user.user_rights_ids_string;

            return Task.FromResult(_dto);
        }

        public Task<List<dto_voice_userC>> GetUsersByGrantType(int grant_type_id,long fs_timestamp)
        {
            string _sql = null;
            List<dto_voice_userC> _ret_List = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_user_page))
            {
                _ret_List = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_ret_List);
            }
            if (fs_timestamp > 0)
            {
                _sql = string.Format("select m.* from {0} as m ," +
                    "{1} as k where k.grant_type_id = {2} and k.grantee_id = m.grantee_id and" +
                    " m.fs_timestamp > {3} ",
                    _table_name,
                    DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                    grant_type_id, fs_timestamp);
            }
            else
            {
                _sql = string.Format("select m.* from {0} as m ," +
                    "{1} as k where k.grant_type_id = {2} and k.grantee_id = m.grantee_id and" +
                    " m.delete_id =0",
                    _table_name,
                    DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                    grant_type_id, fs_timestamp);
            }
            using (var _db = fnn.GetDbConnection())
            {
                _ret_List = _db.Query<voice_userC>(_sql).Select(g => new dto_voice_userC
                {
                    email_address = g.email_address,
                    grantee_id = g.grantee_id,
                    access_token = string.Empty,
                    grantee_name = g.grantee_name,
                    grant_type_id = g.grant_type_id,
                    grant_type_name = g.grant_type_name,
                    phone_no = g.phone_no,
                    user_id = g.user_id,
                    user_login_access_id = g.user_login_access_id,
                    user_login_id = g.user_login_id,
                    user_name = g.user_name,
                    user_rights_ids_string = g.user_rights_ids_string,
                    user_status_id = g.user_status_id,
                    user_type_id = g.user_type_id,
                    user_picture_url = g.user_picture_url,
                    user_level_id = g.user_level_id


                }).ToList();
               
            }
            return Task.FromResult(_ret_List);
        }

        public Task<dto_voice_userC> UpdateB(dto_voice_user_updateC _dto)
        {
            throw new NotImplementedException();
        }
        public Task<dto_voice_userC> AddNewSubAdmin(dto_voice_sub_admin_newC _dto)
        {
            dto_voice_userC _obj_ret = null;
            voice_userC _obj = null;
            if (fnn.LOGGED_USER.user_level != em.user_levelS.facilitator)
            {
                _obj_ret = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_obj_ret);
            }
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_create_new_user))
            {
                _obj_ret = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_obj_ret);
            }
            if (_dto == null)
            {
                AddErrorMessage("Insert Error", "Save Error", "Voice User Object Is Null");
                _obj_ret = null;
                return Task.FromResult(_obj_ret);
            }
            if (string.IsNullOrEmpty(_dto.user_name))
            {
                AddErrorMessage("Insert Error", "Save Error", "User Name Is Null");
                _obj_ret = null;
                return Task.FromResult(_obj_ret);
            }
            if (string.IsNullOrEmpty(_dto.email_address))
            {
                AddErrorMessage("Insert Error", "Save Error", "Email Address is Null");
                return Task.FromResult(_obj_ret);
            }
            #region _image_section
            string _imageUrl = null;
            _obj_ret = null;
            if (!string.IsNullOrEmpty(_dto.user_image_base_64))
            {
                string _imageFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.image_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Image File Extension Cannot Be Empty");
                    return Task.FromResult(_obj_ret);
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
                        AddErrorMessage("Insert Error", "Save Error", "Image File Extension Is Invalid");
                        _obj = null;
                        return Task.FromResult(_obj_ret);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.user_image_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    if (!string.IsNullOrEmpty(rootPath))
                    {
                        string _file_name = string.Format("user-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                        _imageFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                        File.WriteAllBytes(_imageFilePath, _image_bytes);
                        _imageUrl = fnn.CreateImageAssetUrl(_file_name);
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    return Task.FromResult(_obj_ret);
                }
            }
            #endregion
            
            using (var _db = fnn.GetDbContext())
            {
                _obj = new voice_userC();
                _obj.user_name = _dto.user_name.Trim().ToProperCase();
                _obj.phone_no = _dto.phone_no.Trim();
                _obj.email_address = _dto.email_address.Trim();
                _obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                _obj.server_edate = fnn.GetServerDate();
                _obj.user_status_id = em.user_statuS.enabled.ToInt16();
                _obj.user_type_id = em.user_typeS.sub_admin.ToInt16();
                _obj.grantee_id = 0;
                _obj.user_login_id = _dto.email_address;
                _obj.user_login_access_id = em.user_login_accessS.granted.ToInt16();
                _obj.user_login_pwd = fnn.sha256_hash("1234");
                _obj.user_level_id = em.user_levelS.sub_admin.ToInt16();
                _obj.created_by_user_id = fnn.LOGGED_USER.user_id;
                _obj.user_picture_url = _imageUrl;
                _db.VOICE_USERS.Add(_obj);
                var _retVal = _db.SaveChangesWithDuplicateKeyDetected();
                if (_retVal == null || _retVal.Value == true)
                {
                    AddErrorMessage("Duplicate Key Error", "Duplicate Key Error", "You Have Entered A Duplicate Grantee User Name");
                    _obj_ret = null;
                }

            }
            _obj_ret = new dto_voice_userC();
            _obj_ret.grant_type_name = _obj.grant_type_name;
            _obj_ret.grant_type_id = _obj.grant_type_id;
            _obj_ret.phone_no = _obj.phone_no;
            _obj_ret.user_id = _obj.user_id;
            _obj_ret.user_login_access_id = _obj.user_login_access_id;
            _obj_ret.user_name = _obj.user_name;
            _obj_ret.user_rights_ids_string = _obj.user_rights_ids_string;
            _obj_ret.user_status_id = _obj.user_status_id;
            _obj_ret.user_type_id = _obj.user_type_id;
            _obj_ret.email_address = _obj.email_address;
            _obj_ret.user_picture_url = _imageUrl;
            _obj_ret.user_level_id = _obj.user_level_id;

           
            return Task.FromResult(_obj_ret);
        }
    }
}

