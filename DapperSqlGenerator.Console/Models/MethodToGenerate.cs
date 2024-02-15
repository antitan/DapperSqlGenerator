﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.App.Models
{
    public static class MethodNameToGenerate
    {
        public static readonly string GetAllAsync = "GetAllAsync";
        public static readonly string GetPaginatedAsync = "GetPaginatedAsync";
        public static readonly string GetByPkFieldsNamesAsync = "GetBy{pkFieldsNames}Async";
        public static readonly string GetByExpressionAsync = "GetByExpressionAsync";
        public static readonly string InsertAsync = "InsertAsync";
        public static readonly string UpdateAsync = "UpdateAsync";
        public static readonly string DeleteByPkFieldsNamesAsync = "DeleteBy{pkFieldsNames}Async";
        public static readonly string DeleteByExpressionAsync = "DeleteByExpressionAsync";
    }
}
