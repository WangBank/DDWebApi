using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using G3WebApiCore.JWT;
using G3WebApiCore.Model.FreeSqlHelper;
using G3WebApiCore.SwaggerModel;
using FreeSql;
using JWTToken.Filter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace G3WebApiCore
{
    /// <summary>
    /// 主机启动后配置类
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
        /// 读取配置文件
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            string SqlLiteConn = string.Empty;


            SqlLiteConn = Configuration.GetConnectionString("DingDingDb");
       
            var fsql1 = new FreeSqlBuilder().UseConnectionString(DataType.SqlServer, SqlLiteConn)
               .UseAutoSyncStructure(false)
                .Build<SqlServerFlag>();
            services.AddSingleton(fsql1);

            services.AddTransient<ITokenHelper, TokenHelper>();
            //读取配置文件配置的jwt相关配置
            services.Configure<JWTConfig>(Configuration.GetSection("JWTConfig"));
            //启用JWT
            services.AddAuthentication(Options =>
            {
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer();
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
               // c.OperationFilter<AuthTokenHeaderParameter>();// 自动添加token
            });
            services.AddScoped<TokenFilter>();
            services.AddControllers().AddNewtonsoftJson(o=>o.SerializerSettings.DateFormatString="yyyy-MM-dd HH:mm:ss");

            services.AddCors(option => option.AddPolicy("AllowCors", bu => bu.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "钉钉审批单据统计Api");
                c.RoutePrefix = "";//设置根节点访问
                //c.DocExpansion(DocExpansion.None);//折叠
                //c.DefaultModelsExpandDepth(-1);//不显示Schemas
            });


            app.UseRouting();
            app.UseCors("AllowCors");
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
