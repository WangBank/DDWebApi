using DingtalkApprovalApi.Common;
using DingtalkApprovalApi.JWT;
using DingtalkApprovalApi.MiddleWare;
using DingtalkApprovalApi.SwaggerModel;
using FreeSql;
using JWTToken.Filter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DingtalkApprovalApi
{
    /// <summary>
    /// 主机创建之后运行的类
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 获取配置文件类
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 服务注入总入口
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            string SqlServer = string.Empty;


            SqlServer = Configuration.GetConnectionString("DingDingDb");

            var fsql1 = new FreeSqlBuilder().UseConnectionString(DataType.SqlServer, SqlServer)
               .UseAutoSyncStructure(false)
                .Build<SqlServerFlag>();
            services.AddSingleton<IFreeSql>(fsql1);
            services.AddScoped<UnitOfWorkManager>();
            services.AddFreeRepository(null, typeof(Startup).Assembly);
            services.AddTransient<ITokenHelper, TokenHelper>();
            //读取配置文件配置的jwt相关配置
            services.Configure<JWTConfig>(Configuration.GetSection("JWTConfig"));
            //启用JWT
            services.AddAuthentication(Options =>
            {
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                
            }).AddJwtBearer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "api文档",
                    Description = "钉钉审批单据统计Api"
                });
                // 为 Swagger 设置xml文档注释路径
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.DocInclusionPredicate((docName, description) => true);
                //添加对控制器的标签(描述)
                c.DocumentFilter<ApplyTagDescriptions>();//显示类名
                c.CustomSchemaIds(type => type.FullName);// 可以解决相同类名会报错的问题
                c.OperationFilter<AuthTokenHeaderParameter>(); //jwt加token
            });
            services.AddScoped<TokenFilter>();
            services.AddControllers().AddNewtonsoftJson(o => o.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
               
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DingtalkApprovalApi Api");
                c.RoutePrefix = "doc";//设置根节点访问
                //c.DocExpansion(DocExpansion.None);//折叠
                c.DefaultModelsExpandDepth(-1);//不显示Schemas
            });
            app.UseMiddleware<LoggerMiddleware>();
            app.UseRouting();

            app.UseCors(builder =>
            {
                string[] withOrigins = Configuration.GetSection("WithOrigins").Get<string[]>();
                builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(withOrigins);
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), @"Images")),
                RequestPath = new PathString("/Images")
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
