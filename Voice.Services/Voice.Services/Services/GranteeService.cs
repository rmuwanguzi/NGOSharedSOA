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
using Dapper;
namespace Voice.Service
{
    public class GranteeService : IGranteeService
    {
        public string controller_key { get; set; }
        private IMessageDialog _dialog;
        private string _table_name = "grantee_tb";
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
        public GranteeService(IMessageDialog dialog)
        {
            _dialog = dialog;
            _table_name = DbHelper.GetTableSchemaName(_table_name);
        }
        public Task<granteeC> AddNew(dto_grantee_newC _dto)
        {
            granteeC _obj = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_create_new_grantee))
            {
                _obj = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_obj);
            }
            List<int> m_targetGpList = new List<int>();

            if (_dto == null)
            {
                AddErrorMessage("Insert Error", "Save Error", "Grantee Object Is Null");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (_dto.grant_type_id <= 0)
            {
                AddErrorMessage("Insert Error", "Save Error", "Grant Type Id Is Invalid");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (string.IsNullOrEmpty(_dto.target_gp_ids))
            {
                AddErrorMessage("Insert Error", "Save Error", "Grantee Is Not Associated To Any Target Group");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (string.IsNullOrEmpty(_dto.grantee_name))
            {
                AddErrorMessage("Insert Error", "Save Error", "Grantee Name Is Null");
                _obj = null;
                return Task.FromResult(_obj);
            }
            #region _image_section
            string _imageUrl = null;
            if (!string.IsNullOrEmpty(_dto.grantee_image_base_64))
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
                        return Task.FromResult(_obj);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.grantee_image_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("grt-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                    _imageFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                    File.WriteAllBytes(_imageFilePath, _image_bytes);
                    _imageUrl = fnn.CreateImageAssetUrl(_file_name);
                }
                catch (Exception ex)
                {
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    LoggerX.LoggerX.LogException(ex);
                    _obj = null;
                    return Task.FromResult(_obj);
                }
            }
            #endregion

            #region _profile_section
            string _profileUrl = null;
            if (!string.IsNullOrEmpty(_dto.grantee_profile_base_64))
            {
                string _profileFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.grantee_profile_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Profile File Document Extension Cannot Be Empty");
                    _obj = null;
                    return Task.FromResult(_obj);
                }
                try
                {
                    if (_dto.grantee_profile_file_extension.IndexOf('.') == -1)
                    {
                        _file_extension = string.Format(".{0}", _dto.grantee_profile_file_extension);
                    }
                    else
                    {
                        _file_extension = _dto.grantee_profile_file_extension;
                    }
                    if (!fnn.IsValidProfileFileExtension(_file_extension))
                    {
                        AddErrorMessage("Insert Error", "Save Error", "Profile File Extension Is Invalid");
                        _obj = null;
                        return Task.FromResult(_obj);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.grantee_profile_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("_profile-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                    _profileFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                    File.WriteAllBytes(_profileFilePath, _image_bytes);
                    _profileUrl = fnn.CreateImageAssetUrl(_file_name);
                }
                catch (Exception ex)
                {
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    LoggerX.LoggerX.LogException(ex);
                    _obj = null;
                    return Task.FromResult(_obj);
                }
            }
            #endregion

            try
            {
                using (var _db = fnn.GetDbContext())
                {
                    try
                    {
                        var _tg_ids = _dto.target_gp_ids.Split(',');
                        int _id = 0;
                        foreach (var _id_s in _tg_ids)
                        {
                            _id = _id_s.ToInt32();
                            if (_id == 0)
                            {
                                AddErrorMessage("Insert Error", "Save Error", "You Have Entered An Invalid Target Group");
                                _obj = null;
                                return Task.FromResult(_obj);
                            }
                            else
                            {
                                var _retObj = _db.Database.ExecuteScalar(string.Format("select target_gp_id from {0} where target_gp_id={1}", DbHelper.GetTableSchemaName("target_group_tb"), _id));
                                if (_retObj == null || _retObj.ToInt32() == 0)
                                {
                                    AddErrorMessage("Insert Error", "Save Error", "You Have Entered An Invalid Target Group");
                                    _obj = null;
                                    return Task.FromResult(_obj);
                                }
                                m_targetGpList.Add(_id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerX.LoggerX.LogException(ex);
                        AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                        _obj = null;
                        return Task.FromResult(_obj);
                    }
                    var _grant_type = _db.GRANT_TYPES.Where(e => e.grant_type_id == _dto.grant_type_id & e.delete_id == 0).FirstOrDefault();
                    if (_grant_type == null)
                    {
                        AddErrorMessage("Error", "Error", "Unable To Find Grant Type Object");
                    }
                     _obj = new granteeC();
                    _obj.grantee_name = _dto.grantee_name.Trim().ToProperCase();
                    _obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                    _obj.server_edate = fnn.GetServerDate();
                    _obj.company_address = _dto.company_address;
                    _obj.company_email = _dto.company_email;
                    _obj.contact_person = _dto.contact_person;
                    _obj.contact_phone = _dto.contact_phone;
                    _obj.grant_type_id = _dto.grant_type_id;
                    _obj.grant_type_name = _grant_type.grant_type_name;
                    _obj.target_gp_ids = _dto.target_gp_ids.Trim();//16,17,23
                    _obj.created_by_user_id = fnn.LOGGED_USER.user_id;
                    _obj.grantee_image_url = _imageUrl;
                    _obj.grantee_profile_url = _profileUrl;
                    _obj.alias = _dto.alias;
                    _db.GRANTEES.Add(_obj);
                    var _retVal = _db.SaveChangesWithDuplicateKeyDetected();
                    if (_retVal == null || _retVal.Value == true)
                    {
                        AddErrorMessage("Duplicate Key Error", "Duplicate Key Error", "You Have Entered A Duplicate Granteee Name");
                        _obj = null;
                    }

                }
            }
            catch (DbException ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _obj = null;
                return Task.FromResult(_obj);
            }
            catch (Exception ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _obj = null;
                return Task.FromResult(_obj);
            }
            //
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
                            _dictionary.Add("grantee_id", "no_of_grantees");
                            foreach (var _v in _dictionary)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grant_type_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grant_type_id=@v1 and delete_id=0", _v.Key, _table_name);
                                _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grant_type_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grant_type_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("grant_type_tb"), _v.Value, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                            }
                            string _in_string = null;
                            string _json_data = null;
                            assign_grantee_to_target_groupC _assign_obj = null;
                            foreach (var _tg_id in m_targetGpList)
                            {
                                _assign_obj = new assign_grantee_to_target_groupC();
                                _assign_obj.grantee_id = _obj.grantee_id;
                                _assign_obj.grant_type_id = _obj.grant_type_id;
                                _assign_obj.target_gp_id = _tg_id;
                                _assign_obj.server_edate = fnn.GetServerDate();
                                _assign_obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();

                                _dbb.ASSIGN_GRANTEE_TO_TARGET_GROUP.Add(_assign_obj);
                                _dbb.SaveChangesWithDuplicateKeyDetected();


                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _tg_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set no_of_grantees = (no_of_grantees + 1),fs_timestamp=@v2 where target_gp_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("target_group_tb")));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                                //

                                if (string.IsNullOrEmpty(_in_string))
                                {
                                    _in_string = "(" + _tg_id.ToString();
                                }
                                else
                                {
                                    _in_string += string.Format(",{0}", _tg_id);
                                }
                            }
                            if (!string.IsNullOrEmpty(_in_string))
                            {
                                _in_string += string.Format(")");
                                var _ret_list = _dbb.Database.SqlQuery<target_groupC>(string.Format("select * from {0} where target_gp_id in {1} and delete_id=0", DbHelper.GetTableSchemaName("target_group_tb"), _in_string)).Select(g => new
                                {
                                    target_gp_id = g.target_gp_id,
                                    target_gp_name = g.target_gp_name,
                                    alias = g.alias
                                }).ToList();
                                _json_data = Newtonsoft.Json.JsonConvert.SerializeObject(_ret_list);
                            }
                            _para.Clear();
                            _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                            _sql = string.Format("SELECT COUNT(DISTINCT target_gp_id) from {0} where grantee_id=@v1 and delete_id=0", DbHelper.GetTableSchemaName("assign_grantee_to_target_group"));
                            _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                            //
                            _para.Clear();
                            _para.Add(fnn.GetDbParameters("@id", _obj.grantee_id));
                            _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                            _para.Add(fnn.GetDbParameters("@v3", _json_data));
                            _sql = string.Format(string.Format("update {0} set target_gp_count = {1} ,fs_timestamp=@v2 , json_target_groups=@v3 where grantee_id = @id and delete_id = 0", _table_name, _count));
                            _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                            _dbb.SaveChanges();
                            //
                            foreach (var k in Enum.GetValues(typeof(em.resource_typeS)))
                            {
                                var _robj = new voice_resource_categoryC();
                                _robj.created_by_user_id = fnn.LOGGED_USER.user_id;
                                _robj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                                _robj.resource_cat_id = _obj.grantee_id;
                                _robj.resource_cat_img_url = _imageUrl;
                                _robj.resource_cat_name = _obj.grantee_name.ToProperCase();
                                _robj.resource_type_id = k.ToInt16();
                                _robj.resource_cat_type_id = em.resource_cat_typeS.grantee.ToInt32();
                                _robj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                                _robj.server_edate = fnn.GetServerDate();
                                _dbb.VOICE_RESOURCE_CATEGORIES.Add(_robj);
                                _dbb.SaveChanges();
                            }

                        }
                        catch (DbException ex)
                        {
                            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            LoggerX.LoggerX.LogException(ex);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            LoggerX.LoggerX.LogException(ex);
                        }

                    }
                }


            });
            return Task.FromResult(_obj);
        }
        public Task<bool> Delete(int id)
        {
            bool _record_deleted = false;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_delete_grantee))
            {
                 AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_record_deleted);
            }
            granteeC _obj = null;
            using (var _db = fnn.GetDbContext())
            {
                try
                {
                    _obj = _db.GRANTEES.Where(e => e.grantee_id == id & e.delete_id == 0).SingleOrDefault();
                    if (_obj == null)
                    {
                        _record_deleted = false;
                        AddErrorMessage("Delete Error", "Delete Error", "Could Not Find Grantee Object");
                    }
                    else
                    {
                        var _has_dependency = DbHelper.HasDbDependencies(_db.Database, new string[] { "grantee_tb","assign_grantee_to_target_group" }, DbHelper.GetDbSchema(), new string[] { "grantee_id" }, id);
                        if (_has_dependency == null || _has_dependency == true)
                        {
                            AddErrorMessage("Delete Error", "Delete Error", "Unable To Delete Record Because It Has System Dependencies.");
                            _record_deleted = false;
                        }
                        else
                        {
                            var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                            {
                                pk_col_name = "grantee_id",
                                pk_id = _obj.grantee_id,
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
                catch (DbException ex)
                {
                    LoggerX.LoggerX.LogException(ex);
                    _record_deleted = false;
                    return Task.FromResult(_record_deleted);
                }
                catch (Exception ex)
                {
                    LoggerX.LoggerX.LogException(ex);
                    _record_deleted = false;
                    return Task.FromResult(_record_deleted);
                }
                
            }
            if (_record_deleted)
            {
                Task.Factory.StartNew(() =>
                {
                    if (_obj != null)
                    {
                       
                            Caching.CacheManager.ClearCache(Caching.em.cache_keys.grantee);
                        
                        string _sql = null;
                        int _count = 0;
                        List<DbParameter> _para = new List<DbParameter>();
                        Dictionary<string, string> _dictionary = new Dictionary<string, string>();
                        List<int> m_targetGpList = new List<int>();
                        var _tg_ids = _obj.target_gp_ids.Split(',');
                        int _id = 0;
                        foreach (var _id_s in _tg_ids)
                        {
                            _id = _id_s.ToInt32();
                            if (_id > 0)
                            {
                                m_targetGpList.Add(_id);
                            }
                        }
                        using (var _dbb = fnn.GetDbContext())
                        {
                            try
                            {
                                _dictionary.Add("grantee_id", "no_of_grantees");
                                foreach (var _v in _dictionary)
                                {
                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@v1", _obj.grant_type_id));
                                    _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grant_type_id=@v1 and delete_id=0", _v.Key, _table_name);
                                    _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@id", _obj.grant_type_id));
                                    _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                    _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grant_type_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("grant_type_tb"), _v.Value, _count));
                                    _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                    _dbb.SaveChanges();
                                }
                                var _to_delete = (from g in _dbb.ASSIGN_GRANTEE_TO_TARGET_GROUP
                                                  where g.grantee_id == _obj.grantee_id & g.delete_id == 0
                                                  select g).ToList();
                                foreach (var _rec in _to_delete)
                                {
                                    var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                                    {
                                        pk_col_name = "un_id",
                                        pk_id = _rec.un_id,
                                        table_name = DbHelper.GetTableSchemaName("assign_grantee_to_target_group")
                                    }, _dbb);
                                    if (_result == null || _result == false)
                                    {
                                        System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                                        System.Diagnostics.Debug.WriteLine("DELETE ASSIGNED OBJECT FAILED");
                                    }
                                    else
                                    {
                                        _dbb.SaveChanges();
                                    }
                                    _para.Clear();
                                    _para.Add(fnn.GetDbParameters("@id", _rec.target_gp_id));
                                    _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                    _sql = string.Format(string.Format("update {0} set no_of_grantees = (no_of_grantees - 1),fs_timestamp=@v2 where target_gp_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("target_group_tb")));
                                    _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                    _dbb.SaveChanges();
                                }

                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grant_type_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT target_gp_id) from {0} where grant_type_id=@v1 and delete_id=0", DbHelper.GetTableSchemaName("assign_grantee_to_target_group"));
                                _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grant_type_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                //
                                _sql = string.Format(string.Format("update {0} set no_of_target_groups = {1},fs_timestamp=@v2 where grant_type_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("grant_type_tb"), _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                                //
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT target_gp_id) from {0} where grantee_id=@v1 and delete_id=0", DbHelper.GetTableSchemaName("assign_grantee_to_target_group"));
                                _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grantee_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                //
                                _sql = string.Format(string.Format("update {0} set target_gp_count = {1},fs_timestamp=@v2 where grantee_id = @id and delete_id = 0", _table_name, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();

                                int _cat_type_id = em.resource_cat_typeS.grantee.ToInt32();
                                var _r_list = (from k in _dbb.VOICE_RESOURCE_CATEGORIES
                                               where k.resource_cat_type_id == _cat_type_id & k.resource_cat_id == _obj.grantee_id
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
                            catch (DbException ex)
                            {
                                LoggerX.LoggerX.LogException(ex);
                                
                                
                            }
                            catch (Exception ex)
                            {
                                LoggerX.LoggerX.LogException(ex);
                                
                            }
                        }

                    }
                });
               
            }
            return Task.FromResult(_record_deleted);
        }
        public Task<List<granteeC>> GetAll()
        {
            List<granteeC> _list = null;
            string _sql = null;
            try
            {
              // _list = Caching.CacheManager.GetCachedItems<List<granteeC>>(Caching.em.cache_keys.grantee);
                if (_list == null || _list.Count == 0)
                {

                    using (var _conn = fnn.GetDbConnection())
                    {
                        _sql = string.Format("select * from {0} where delete_id=0", _table_name);
                        _list = _conn.Query<granteeC>(_sql).ToList();
                        Task.Factory.StartNew(() =>
                        {
                            Caching.CacheManager.SaveItems<List<granteeC>>(Caching.em.cache_keys.grantee, _list);
                        });
                    }
                  
                }
            }
            catch (DbException ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _list = null;
            }
            catch (Exception ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _list = null;
            }
            return Task.FromResult(_list);
        }
        public Task<List<granteeC>> GetAll(long fs_timestamp)
        {
            List<granteeC> _list = null;
            try
            {
                if (fs_timestamp == 0)
                {
                  // _list = Caching.CacheManager.GetCachedItems<List<granteeC>>(Caching.em.cache_keys.grantee);
                    if (_list != null & _list.Count > 0)
                    {
                        return Task.FromResult(_list);
                    }
                }
                string _sql = null;
                using (var _conn = fnn.GetDbConnection())
                {
                    if (fs_timestamp == 0)
                    {
                        _sql = string.Format("select * from {0} where delete_id=0", DbHelper.GetTableSchemaName(_table_name));
                        _list = _conn.Query<granteeC>(_sql).ToList();
                        Task.Factory.StartNew(() =>
                        {
                            Caching.CacheManager.SaveItems<List<granteeC>>(Caching.em.cache_keys.grantee, _list);
                        });
                    }
                    else
                    {
                        _sql = string.Format("select * from {0} where fs_timestamp>{1}", DbHelper.GetTableSchemaName(_table_name), fs_timestamp);
                        _list = _conn.Query<granteeC>(_sql).ToList();
                    }

                }
            }
            catch (DbException ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _list = null;
            }
            catch (Exception ex)
            {
                LoggerX.LoggerX.LogException(ex);
                _list = null;
            }
            return Task.FromResult(_list);
        }
        public Task<granteeC> GetSingle(int id)
        {
            granteeC _obj = null;
            try
            {
                using (var _db = fnn.GetDbContext())
                {
                   // _db.MapClassToTableName<granteeC>("grantee_tb");
                    _obj = _db.GRANTEES.Where(e => e.grantee_id == id & e.delete_id == 0).FirstOrDefault();
                    if (_obj == null)
                    {
                        AddErrorMessage("Search Error", "Error", "Unable to Find Record");
                        _obj = null;
                    }

                }
            }
            catch (KellermanSoftware.NetDataAccessLayer.SqlException ex)
            {
                LoggerX.LoggerX.LogException(ex);
            }
            catch (Exception ex)
            {
                LoggerX.LoggerX.LogException(ex);
            }
            return Task.FromResult(_obj);
        }
        public Task<bool> Update(dto_grantee_update _dto)
        {
            bool _updated = false;

            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_grantee_info))
            {
                _updated = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_updated);
            }
            string _old_string_ids = null;
            if (string.IsNullOrEmpty(_dto.grantee_name))
            {
                AddErrorMessage("Update Error", "Save Error", "Facilitator Name Is Null!");
                return Task.FromResult(_updated);
            }

            if (string.IsNullOrEmpty(_dto.target_gp_ids))
            {
                AddErrorMessage("Insert Error", "Save Error", "Grantee Is Not Associated To Any Target Group");
                return Task.FromResult(_updated);
            }

            #region _image_section
            string _imageUrl = null;
            if (!string.IsNullOrEmpty(_dto.grantee_image_base_64))
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
                    var _image_bytes = System.Convert.FromBase64String(_dto.grantee_image_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("grt-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                    _imageFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                    File.WriteAllBytes(_imageFilePath, _image_bytes);
                    _imageUrl = fnn.CreateImageAssetUrl(_file_name);
                }
                catch (Exception ex)
                {
                    LoggerX.LoggerX.LogException(ex);
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    return Task.FromResult(_updated);
                }
            }
            #endregion
            #region _image_section
            string _profileUrl = null;
            if (!string.IsNullOrEmpty(_dto.grantee_profile_base_64))
            {
                string _profileFilePath = string.Empty;
                string _file_extension = string.Empty;
                if (string.IsNullOrEmpty(_dto.grantee_profile_file_extension))
                {
                    AddErrorMessage("Insert Error", "Save Error", "Profile File Extension Cannot Be Empty");
                    return Task.FromResult(_updated);
                }
                try
                {
                    if (_dto.grantee_profile_file_extension.IndexOf('.') == -1)
                    {
                        _file_extension = string.Format(".{0}", _dto.grantee_profile_file_extension);
                    }
                    else
                    {
                        _file_extension = _dto.grantee_profile_file_extension;
                    }

                    if (!fnn.IsValidProfileFileExtension(_file_extension))
                    {
                        AddErrorMessage("Insert Error", "Save Error", "Profile File Extension Is Invalid");
                        return Task.FromResult(_updated);
                    }
                    var _image_bytes = System.Convert.FromBase64String(_dto.grantee_profile_base_64);
                    if (_image_bytes == null || _image_bytes.Length == 0)
                    {
                        throw new Exception("Error Getting Image From Base64 String");
                    }
                    var rootPath = HostingEnvironment.MapPath("~/ImageAssets/");
                    string _file_name = string.Format("_profile-{0:yyyy-MM-dd_hh-mm-ss-tt}{1}", DateTime.Now, _file_extension);
                    _profileFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
                    File.WriteAllBytes(_profileFilePath, _image_bytes);
                    _profileUrl = fnn.CreateImageAssetUrl(_file_name);
                }
                catch (Exception ex)
                {
                    LoggerX.LoggerX.LogException(ex);
                    AddErrorMessage("Thrown Execption", "Thrown Exception", ex.Message);
                    return Task.FromResult(_updated);
                }
            }
            #endregion

            bool _update_target_ids = false;
            List<int> _to_remove = new List<int>();
            List<int> _to_add = new List<int>();
            granteeC _obj = null;
            try
            {
                using (var _trans = new ZUpdateContext())
                {
                    var _existing = _trans.Context.GRANTEES.Where(e => e.grantee_id == _dto.grantee_id & e.delete_id == 0).FirstOrDefault();
                    if (_existing == null)
                    {
                        _updated = false;
                        AddErrorMessage("Update Error", "Save Error", "Unable To Find Grantee Object");
                        return Task.FromResult(_updated);
                    }
                    _obj = _existing;
                   
                    _old_string_ids = _existing.target_gp_ids;
                    if (_old_string_ids.Trim().ToLower() != _dto.target_gp_ids.Trim().ToLower())
                    {
                        _update_target_ids = true;
                        var _old_v = _old_string_ids.Split(',');
                        var _new_v = _dto.target_gp_ids.Split(',');

                        foreach (var _v in _old_v)
                        {
                            if (!_new_v.Contains(_v))
                            {
                                _to_remove.Add(_v.ToInt32());
                            }
                        }
                        foreach (var _v in _new_v)
                        {
                            if (!_old_v.Contains(_v))
                            {
                                _to_add.Add(_v.ToInt32());
                            }
                        }
                        List<DbParameter> _para = new List<DbParameter>();
                        foreach (var _v in _to_remove)
                        {
                            var _assign_obj = (from d in _trans.Context.ASSIGN_GRANTEE_TO_TARGET_GROUP
                                               where d.grantee_id == _existing.grantee_id & d.target_gp_id == _v
                                               select d).FirstOrDefault();
                            if (_assign_obj != null)
                            {
                                var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                                {
                                    pk_col_name = "un_id",
                                    pk_id = _assign_obj.un_id,
                                    table_name = DbHelper.GetTableSchemaName("assign_grantee_to_target_group")
                                }, _trans.Context);
                                if (_result == null || _result == false)
                                {
                                    System.Diagnostics.Debug.WriteLine("<<ERROR>>");
                                    System.Diagnostics.Debug.WriteLine("DELETE ASSIGNED OBJECT FAILED");
                                }
                                else
                                {
                                    _trans.Context.SaveChanges();
                                }
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _v));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                var _sql = string.Format(string.Format("update {0} set no_of_grantees = (no_of_grantees - 1),fs_timestamp=@v2 where target_gp_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("target_group_tb")));
                                _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _trans.Context.SaveChanges();
                            }
                        }
                    }
                    if (_existing.grantee_name.ToLower() != _dto.grantee_name.ToLower())
                    {
                        var _ret = DbHelper.UpdatePrimaryKeyColumn(new DbHelperPrimarykeyUpdateC
                        {
                            col_to_update = "grantee_name",
                            new_col_value = _dto.grantee_name.Trim().ToProperCase(),
                            table_name = _table_name,
                            pk_col_name = "grantee_id",
                            pk_id = _dto.grantee_id
                        }, _trans.Context);
                        if (_ret == null || _ret.Value == false)
                        {
                            AddErrorMessage("Error", "Update Error", "Grantee Name Already Exists");
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
                        if (_updated)
                        {
                            List<DbParameter> _para = new List<DbParameter>();
                            _para.Clear();
                            _para.Add(fnn.GetDbParameters("@id", _existing.grantee_id));
                            _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                            _para.Add(fnn.GetDbParameters("@v3", string.IsNullOrEmpty(_dto.alias) ? string.Empty : _dto.alias));
                            _para.Add(fnn.GetDbParameters("@v4", string.IsNullOrEmpty(_imageUrl) ? string.Empty : _imageUrl));
                            _para.Add(fnn.GetDbParameters("@v5", _dto.target_gp_ids));
                            _para.Add(fnn.GetDbParameters("@v6", string.IsNullOrEmpty(_dto.company_address) ? string.Empty : _dto.company_address));
                            _para.Add(fnn.GetDbParameters("@v7", string.IsNullOrEmpty(_dto.company_email) ? string.Empty : _dto.company_email));
                            _para.Add(fnn.GetDbParameters("@v8", string.IsNullOrEmpty(_dto.contact_person)?string.Empty:_dto.contact_person));
                            _para.Add(fnn.GetDbParameters("@v9", string.IsNullOrEmpty(_dto.contact_phone) ? string.Empty : _dto.contact_phone));
                            _para.Add(fnn.GetDbParameters("@v10", string.IsNullOrEmpty(_profileUrl) ? string.Empty : _profileUrl));
                            //
                            var _sql = string.Format(string.Format("update {0} set fs_timestamp=@v2,alias=@v3,grantee_image_url=@v4" +
                                ",target_gp_ids=@v5,company_address=@v6,company_email=@v7," +
                                "contact_person=@v8,contact_phone=@v9,grantee_profile_url=@v10 " +
                                "where grantee_id = @id and delete_id = 0", _table_name));
                            _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                            _trans.Context.SaveChanges();
                            //
                            int _cat_type_id = em.resource_cat_typeS.grantee.ToInt32();
                            var _r_list = (from k in _trans.Context.VOICE_RESOURCE_CATEGORIES
                                           where k.resource_cat_type_id == _cat_type_id & k.resource_cat_id == _obj.grantee_id
                                           & k.delete_id == 0
                                           select k).ToList();
                            foreach (var _v in _r_list)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _existing.grantee_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _para.Add(fnn.GetDbParameters("@v3", string.IsNullOrEmpty(_imageUrl) ? string.Empty : _imageUrl));
                                _para.Add(fnn.GetDbParameters("@v4", _dto.grantee_name.Trim().ToProperCase()));
                                _sql = string.Format(string.Format("update {0} set fs_timestamp=@v2,resource_cat_img_url=@v3" +
                                  ",resource_cat_name=@v4 where un_id = @v1 and delete_id = 0", DbHelper.GetTableSchemaName("voice_resource_cat_tb")));
                                _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _trans.Context.SaveChanges();
                            }
                        }
                        _trans.Commit();
                        Task.Factory.StartNew(() =>
                        {
                            Caching.CacheManager.ClearCache(Caching.em.cache_keys.grantee);
                        });
                    }
                }
            }
            catch (DbException ex)
            {
                _updated = false;
                LoggerX.LoggerX.LogException(ex);
                return Task.FromResult(_updated);
            }
            catch (Exception ex)
            {
                _updated = false;
                LoggerX.LoggerX.LogException(ex);
                return Task.FromResult(_updated);
            }
            if (_update_target_ids)
            {
                Task.Factory.StartNew(() =>
                {

                    string _sql = null;
                    int _count = 0;
                    List<DbParameter> _para = new List<DbParameter>();
                    Dictionary<string, string> _dictionary = new Dictionary<string, string>();
                    using (var _dbb = fnn.GetDbContext())
                    {
                        try
                        {
                            _dictionary.Add("grantee_id", "no_of_grantees");
                            foreach (var _v in _dictionary)
                            {
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@v1", _obj.grant_type_id));
                                _sql = string.Format("SELECT COUNT(DISTINCT {0}) from {1} where grant_type_id=@v1 and delete_id=0", _v.Key, _table_name);
                                _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _obj.grant_type_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set {1} = {2},fs_timestamp=@v2 where grant_type_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("grant_type_tb"), _v.Value, _count));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                            }
                            string _in_string = null;
                            string _json_data = null;
                            assign_grantee_to_target_groupC _assign_obj = null;
                            foreach (var _tg_id in _to_add)
                            {
                                _assign_obj = new assign_grantee_to_target_groupC();
                                _assign_obj.grantee_id = _obj.grantee_id;
                                _assign_obj.grant_type_id = _obj.grant_type_id;
                                _assign_obj.target_gp_id = _tg_id;
                                _assign_obj.server_edate = fnn.GetServerDate();
                                _assign_obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();

                                _dbb.ASSIGN_GRANTEE_TO_TARGET_GROUP.Add(_assign_obj);
                                _dbb.SaveChangesWithDuplicateKeyDetected();
                                _para.Clear();
                                _para.Add(fnn.GetDbParameters("@id", _tg_id));
                                _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                                _sql = string.Format(string.Format("update {0} set no_of_grantees = (no_of_grantees + 1),fs_timestamp=@v2 where target_gp_id = @id and delete_id = 0", DbHelper.GetTableSchemaName("target_group_tb")));
                                _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                                _dbb.SaveChanges();
                                //
                                if (string.IsNullOrEmpty(_in_string))
                                {
                                    _in_string = "(" + _tg_id.ToString();
                                }
                                else
                                {
                                    _in_string += string.Format(",{0}", _tg_id);
                                }
                            }
                            if (!string.IsNullOrEmpty(_in_string))
                            {
                                _in_string += string.Format(")");
                                var _ret_list = _dbb.Database.SqlQuery<target_groupC>(string.Format("select * from {0} where target_gp_id in {1} and delete_id=0", DbHelper.GetTableSchemaName("target_group_tb"), _in_string)).Select(g => new
                                {
                                    target_gp_id = g.target_gp_id,
                                    target_gp_name = g.target_gp_name,
                                    alias = g.alias
                                }).ToList();
                                _json_data = Newtonsoft.Json.JsonConvert.SerializeObject(_ret_list);
                            }
                            _para.Clear();
                            _para.Add(fnn.GetDbParameters("@v1", _obj.grantee_id));
                            _sql = string.Format("SELECT COUNT(DISTINCT target_gp_id) from {0} where grantee_id=@v1 and delete_id=0", DbHelper.GetTableSchemaName("assign_grantee_to_target_group"));
                            _count = _dbb.Database.ExecuteScalar(_sql, _para.ToArray()).ToInt32();
                            //
                            _para.Clear();
                            _para.Add(fnn.GetDbParameters("@id", _obj.grantee_id));
                            _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                            _para.Add(fnn.GetDbParameters("@v3", _json_data));
                            _sql = string.Format(string.Format("update {0} set target_gp_count = {1} ,fs_timestamp=@v2 , json_target_groups=@v3 where grantee_id = @id and delete_id = 0", _table_name, _count));
                            _dbb.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                            _dbb.SaveChanges();
                            //


                        }
                        catch (DbException ex)
                        {
                            LoggerX.LoggerX.LogException(ex);
                        }
                        catch (Exception ex)
                        {
                            LoggerX.LoggerX.LogException(ex);
                        }

                    }

                });
            }
            return Task.FromResult(_updated);
        }
        public Task<List<granteeC>> GetAllGranteesByTargetGroup(int target_gp_id)
        {
            List<granteeC> _granteeList = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_grantee_page))
            {
                _granteeList = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_granteeList);
            }
            
            var _sql = string.Format("select m.* from {0} as m, {1}  as c" +
                " where c.target_gp_id= {2} and m.grantee_id = c.grantee_id and m.delete_id = 0 ",
                _table_name,
                DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                target_gp_id);
            using (var _db = fnn.GetDbConnection())
            {
                _granteeList = _db.Query<granteeC>(_sql).ToList();
            }
            return Task.FromResult(_granteeList);
        }
        public Task<List<granteeC>> GetAllGranteesByGrantType(int grant_type_id)
        {
            List<granteeC> _granteeList = null;
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_view_grantee_page))
            {
                _granteeList = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_granteeList);
            }
            var _sql = string.Format("select m.* from {0} as m, {1}  as c" +
                " where c.grant_type_id= {2} and m.grantee_id = c.grantee_id and m.delete_id = 0 ",
                _table_name,
                DbHelper.GetTableSchemaName("assign_grantee_to_target_group"),
                grant_type_id);
            using (var _db = fnn.GetDbConnection())
            {
                _granteeList = _db.Query<granteeC>(_sql).ToList();
            }
            return Task.FromResult(_granteeList);
        }
    }
}

