﻿using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac.Integration.Mvc;
using SqlSugar;
using System.Configuration;
using BackstageManagement.Model.Context;

namespace BackstageManagement
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BackstageManagement.Model.Context.DbContext dbContext = new BackstageManagement.Model.Context.DbContext();
            DbSend.Send(dbContext);
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            var basepath = AppDomain.CurrentDomain.RelativeSearchPath;

            builder.Register<ISqlSugarClient>(c =>
            {
                var sqlSugarClient = new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["conn"].ToString(),
                    DbType = DbType.MySql,
                    InitKeyType = InitKeyType.Attribute,
                    IsAutoCloseConnection = true,
                });

                sqlSugarClient.Aop.OnError = (ex) =>
                {
                    //这边不能记录数据库，异常没抛
                };
                return sqlSugarClient;
            });
            var repositoryDllFile = Path.Combine(basepath, "BackstageManagement.Repository.dll");
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces();

            var servicesDllFile = Path.Combine(basepath, "BackstageManagement.Services.dll");
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices).AsImplementedInterfaces();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
        protected void Application_Error()
        {

        }

    }
}
