﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.SwaggerModel
{
    /// <summary>
    /// Swagger description
    /// </summary>
    public class ApplyTagDescriptions : IDocumentFilter
    {
        /// <summary>
        /// apply 方法
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag{Name ="ApplyBillInfo",Description="按单据统计api"},
                new OpenApiTag{Name ="ApproverInfo",Description="按审批人统计api"},
                 new OpenApiTag{Name ="BaseInfo",Description="基础信息api"}
            };
        }
    }
}
