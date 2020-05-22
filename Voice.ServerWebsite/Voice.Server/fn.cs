using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using Voice.Shared.Core.Interfaces;
using Voice.Shared.Core;
using Voice.Service;
using Autofac;
using Voice.Service.Interfaces;
using System.Windows.Forms;
using Autofac.Core;
using System.Web.Hosting;

namespace Voice.Server
{
    public static class fn
    {
        
        internal static void Injection()
        {

            //injection        
            fnn.DB_PROVIDER = em.db_Type.sql_server;
            var builder = new ContainerBuilder();
            builder.RegisterType<GrantTypeService>().As<IGrantTypeService>().SingleInstance();
            builder.RegisterType<TargetGroupService>().As<ITargetGroupService>().SingleInstance();
            builder.RegisterType<GranteeService>().As<IGranteeService>().SingleInstance();
            builder.RegisterType<VoiceUserService>().As<IVoiceUserService>().SingleInstance();
            builder.RegisterType<LoginService>().As<ILoginService>().SingleInstance();
            builder.RegisterType<TokenService>().As<ITokenService>().SingleInstance();
            builder.RegisterType<ResourceService>().As<IResourceService>().SingleInstance();
            builder.RegisterType<MyDriveResourceService>().As<IMyDriveResourceService>().SingleInstance();
            builder.RegisterType<Platform_Specific.LoggedInUserService>().As<ILoggedInUserService>().SingleInstance();
            Platform_Specific.MessageDialog _messaging = new Platform_Specific.MessageDialog();
            builder.RegisterInstance(_messaging).As<IMessageDialog>().SingleInstance();
            //
            Platform_Specific.PCLSettings _pcl = new Platform_Specific.PCLSettings();
            builder.RegisterInstance(_pcl).As<IPCLSettings>().SingleInstance();

            if (fnn.DB_PROVIDER == em.db_Type.vistadb)
            {
                Platform_Specific.VistaDbContextHelper _dbContextHelper = new Platform_Specific.VistaDbContextHelper();
                builder.RegisterInstance(_dbContextHelper).As<IDbContextHelper>().SingleInstance();
            }
            else
            {
                Platform_Specific.SqlServerDbContextHelper _dbContextHelper = new Platform_Specific.SqlServerDbContextHelper();
                builder.RegisterInstance(_dbContextHelper).As<IDbContextHelper>().SingleInstance();
                //
                Platform_Specific.KellerManSqlServerContext _dbKellerSqlServerContextHelper = new Platform_Specific.KellerManSqlServerContext();
                builder.RegisterInstance(_dbKellerSqlServerContextHelper).As<Voice.Service.Interfaces.IKellerDBContext>().SingleInstance();
            }
            try
            {
                ObjectBase._container = builder.Build();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            ObjectBase._container.Resolve<IPCLSettings>().BaseUrl = "";
            if (fnn.DB_PROVIDER == em.db_Type.vistadb)
            {
                //
                var _db_folder_path = HostingEnvironment.MapPath("~/App_Data/");
                
                bool _exists = Directory.Exists((string.Format("{0}", _db_folder_path)));
                if(_exists)
                {
                    var _file_path = System.IO.Path.Combine(_db_folder_path, "oxfarm_db.VDB5");
                    if (System.IO.File.Exists(_file_path))
                    {
                        System.IO.File.Delete(_file_path);
                    }
                    Assembly m_assembly;
                    m_assembly = Assembly.GetExecutingAssembly();
                    var l_stream = m_assembly.GetManifestResourceStream("Voice.Server.oxfarm_db_dev.dbxm");//add the namespace use embedded resource
                    try
                    {
                        VistaSchemaHelper.fnn.UpdateDatabaseSchema("pjk", l_stream, true, _db_folder_path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                    fnn.SeedFacilitator(Voice.Shared.Core.ObjectBase._container.Resolve<IDbContextHelper>().GetContext(), "vicky", "vicky@gmail.com");
                }
               

            }
            
        }
        
    }
}