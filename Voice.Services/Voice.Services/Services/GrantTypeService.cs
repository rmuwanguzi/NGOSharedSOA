
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voice.Shared.Core;
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Interfaces;
using Voice.Shared.Core.Models;
using Dapper;
namespace Voice.Service
{
    public class GrantTypeService : Voice.Shared.Core.Interfaces.IGrantTypeService
    {
        private string _table_name = "grant_type_tb";
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
        public GrantTypeService(IMessageDialog dialog)
        {
            _dialog = dialog;
            _table_name = DbHelper.GetTableSchemaName(_table_name);
        }
        public Task<grant_typeC> AddNew(dto_grant_type_newC _dto)
        {
            grant_typeC _obj = null;
            if (_dto == null)
            {
                AddErrorMessage("Insert Error", "Save Error", "Grant Type Object Is Null");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (string.IsNullOrEmpty(_dto.grant_type_name))
            {
                AddErrorMessage("Insert Error", "Save Error", "Grant Type Name Is Null");
                _obj = null;
                return Task.FromResult(_obj);
            }
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_create_new_grant))
            {
                _obj = null;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_obj);
            }
            using (var _db = fnn.GetDbContext())
            {
                _obj = new grant_typeC();
                _obj.grant_type_name = _dto.grant_type_name.Trim().ToProperCase();
                _obj.fs_timestamp = fnn.GetServerDate().ToUnixTimestamp();
                _obj.server_edate = fnn.GetServerDate();
                _obj.created_by_user_id = fnn.LOGGED_USER.user_id;
                _db.GRANT_TYPES.Add(_obj);
                var _retVal = _db.SaveChangesWithDuplicateKeyDetected();
                if (_retVal == null || _retVal.Value == true)
                {
                    AddErrorMessage("Duplicate Key Error", "Duplicate Key Error", "You Have Entered A Duplicate Grant Type Name");
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
                        _robj.resource_cat_id = _obj.grant_type_id;
                        _robj.resource_cat_type_id = em.resource_cat_typeS.grant_type.ToInt32();
                        _robj.resource_type_id = k.ToInt16();
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
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_delete_grant))
            {
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_record_deleted);
            }
            grant_typeC _obj = null;
            using (var _db = fnn.GetDbContext())
            {
                _obj = _db.GRANT_TYPES.Where(e => e.grant_type_id == id & e.delete_id == 0).SingleOrDefault();
                if (_obj == null)
                {
                    _record_deleted = false;
                    AddErrorMessage("Delete Error", "Delete Error", "Could Not Find Grant Type Object");
                }
                else
                {
                    var _has_dependency = DbHelper.HasDbDependencies(_db.Database, new string[] { "grant_type_tb" }, DbHelper.GetDbSchema(), new string[] { "grant_type_id" }, id);
                    if (_has_dependency == null || _has_dependency == true)
                    {
                        AddErrorMessage("Delete Error", "Delete Error", "Unable To Delete Record Because It Has System Dependencies.");
                        _record_deleted = false;
                    }
                    else
                    {
                        var _result = DbHelper.DeleteRecordWithDeleteId(new DbHelperDeleteRecordC()
                        {
                            pk_col_name = "grant_type_id",
                            pk_id = _obj.grant_type_id,
                            table_name = DbHelper.GetTableSchemaName(_table_name)
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
            if(_record_deleted)
            {
                Task.Factory.StartNew(() =>
                {
                    using (var _dbb = fnn.GetDbContext())
                    {
                        int _cat_type_id = em.resource_cat_typeS.grant_type.ToInt32();
                        var _r_list = (from k in _dbb.VOICE_RESOURCE_CATEGORIES
                                       where k.resource_cat_type_id == _cat_type_id & k.resource_cat_id == _obj.grant_type_id
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
        public Task<List<grant_typeC>> GetAll()
        {
            //using (var _db = fnn.GetDbContext())
            //{
            //   // _db.MapClassToTableName<grant_typeC>(_table_name);
            //    var _list = _db.GRANT_TYPES.Where(e => e.delete_id == 0).ToList();
            //    return Task.FromResult(_list);
            //}

            using (var _conn = fnn.GetDbConnection())
            {
                return Task.FromResult(_conn.Query<grant_typeC>(string.Format("select * from {0} where delete_id=0", _table_name)).ToList());
            }
        }
        public Task<List<grant_typeC>> GetAll(long fs_timestamp)
        {
            List<grant_typeC> _list = null;
            using (var _conn = fnn.GetDbConnection())
            {
                if (fs_timestamp == 0)
                {
                    return Task.FromResult(_conn.Query<grant_typeC>(string.Format("select * from {0} where delete_id=0", _table_name)).ToList());
                }
                else
                {
                    
                  return Task.FromResult(_conn.Query<grant_typeC>(string.Format("select * from {0} where fs_timestamp>{1}", _table_name, fs_timestamp)).ToList());
                    
                }
            }
           // return Task.FromResult(_list);
        }
        public Task<grant_typeC> GetSingle(int id)
        {
            using (var _conn = fnn.GetDbConnection())
            {
                return Task.FromResult(_conn.Query<grant_typeC>(string.Format("select * from {0} where grant_type_id={1} and delete_id=0", _table_name, id)).FirstOrDefault());
            }
            
        }
        public Task<bool> Update(dto_grant_type_updateC _dto)
        {
            bool _updated = false;
            if (string.IsNullOrEmpty(_dto.grant_type_name))
            {
                AddErrorMessage("Update Error", "Save Error", "Grant Type Name Is Null!");
                return Task.FromResult(_updated);
            }
            if (fnn.LOGGED_USER.DoesNotHaveRight(em.right_menu_types.can_edit_grant_info))
            {
                _updated = false;
                AddErrorMessage("Limited Rights Error", "Limited Rights Error", "You Are Not Authorized To Perform This Operation");
                return Task.FromResult(_updated);
            }
            using (var _trans = new ZUpdateContext())
            {
                var _existing = _trans.Context.GRANT_TYPES.Where(e => e.grant_type_id == _dto.grant_type_id & e.delete_id == 0).FirstOrDefault();
                if (_existing == null)
                {
                    _updated = false;
                    AddErrorMessage("Update Error", "Save Error", "Unable To Find Grant Type Object");
                    return Task.FromResult(_updated);
                }
                if (_existing.grant_type_name.ToLower() != _dto.grant_type_name.ToLower())
                {
                    var _ret = DbHelper.UpdatePrimaryKeyColumn(new DbHelperPrimarykeyUpdateC
                    {
                        col_to_update = "grant_type_name",
                        new_col_value = _dto.grant_type_name.Trim().ToProperCase(),
                        table_name = DbHelper.GetTableSchemaName(_table_name),
                        pk_col_name = "grant_type_id",
                        pk_id = _dto.grant_type_id
                    }, _trans.Context);
                    if (_ret == null || _ret.Value == false)
                    {
                        AddErrorMessage("Error", "Update Error", "Grant Type Name Already Exists");
                        _updated = false;
                        return Task.FromResult(_updated);
                    }
                    else
                    {
                        _updated = true;

                        _trans.Context.SaveChanges();
                        _trans.Commit();
                    }
                }
                _updated = true;
                if(_updated)
                {
                    List<DbParameter> _para = new List<DbParameter>();
                    _para.Clear();
                    int _cat_type_id = em.resource_cat_typeS.grant_type.ToInt32();
                    var _r_list = (from k in _trans.Context.VOICE_RESOURCE_CATEGORIES
                                   where k.resource_cat_type_id == _cat_type_id & k.resource_cat_id == _existing.grant_type_id
                                   & k.delete_id == 0
                                   select k).ToList();
                    foreach (var _v in _r_list)
                    {
                        _para.Clear();
                        _para.Add(fnn.GetDbParameters("@v1", _existing.grant_type_id));
                        _para.Add(fnn.GetDbParameters("@v2", fnn.GetServerDate().ToUnixTimestamp()));
                        _para.Add(fnn.GetDbParameters("@v3", System.DBNull.Value));
                        _para.Add(fnn.GetDbParameters("@v4", _dto.grant_type_name.Trim().ToProperCase()));
                       var _sql = string.Format(string.Format("update {0} set fs_timestamp=@v2,resource_cat_img_url=@v3" +
                          ",resource_cat_name=@v4 where un_id = @v1 and delete_id = 0", DbHelper.GetTableSchemaName("voice_resource_cat_tb")));
                        _trans.Context.Database.ExecuteSqlCommand(_sql, _para.ToArray());
                        _trans.Context.SaveChanges();
                    }
                }
              
            }
            return Task.FromResult(_updated);
        }
    }
}
