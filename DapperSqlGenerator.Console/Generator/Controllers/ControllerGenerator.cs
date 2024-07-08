using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator;
using DapperSqlGenerator.App.Models;
using Microsoft.SqlServer.Dac.Model; 

namespace DapperSqlGenerator.App.Generator.Controllers
{
    public class ControllerGenerator : IGenerate, IGenerateClass
    {
        TSqlObject table; 
        string serviceNamespace;  
        string[] refTables;
        string controllerNamespace;
        string mapperNamespace;
        string requestNamespace;
        string responseNamespace;
        string modelNamespace;

        public ControllerGenerator(string modelNamespace,string controllerNamespace,string mapperNamespace, string requestNamespace, string responseNamespace, string serviceNamespace,  string[] refTables, TSqlObject table)
        {
            this.modelNamespace = modelNamespace;
            this.table = table; 
            this.mapperNamespace = mapperNamespace;
            this.serviceNamespace = serviceNamespace; 
            this.refTables = refTables;
            this.controllerNamespace = controllerNamespace;
            this.requestNamespace = requestNamespace;
            this.responseNamespace = responseNamespace;
        }

        public string Generate()
        {
            return "using Microsoft.Extensions.Logging;" + Environment.NewLine +
                    $"using Microsoft.AspNetCore.Authorization;" + Environment.NewLine +
                    $"using Microsoft.AspNetCore.Mvc;" + Environment.NewLine +
                    $"using System;" + Environment.NewLine +
                    $"using System.Net;" + Environment.NewLine +
                    $"using System.Threading.Tasks;" + Environment.NewLine +
                    $"using {modelNamespace};" + Environment.NewLine +
                    $"using {requestNamespace};" + Environment.NewLine +
                    $"using {responseNamespace};" + Environment.NewLine +
                    $"using {serviceNamespace};" + Environment.NewLine +
                    $"using {mapperNamespace};" + Environment.NewLine + Environment.NewLine +
                    $@"namespace {controllerNamespace} {{ 
                        {GenerateClass()}
                    }}";
        }
       

        public string GenerateClassPart()
        {
            return Generate();
        }

        /// <summary>
        /// Get the declaration for the repo class
        /// </summary>
        /// <returns></returns>
        private string GenerateClass()
        {
            var entityClassName = table.Name.Parts[1];
            var controllerName = entityClassName.PascalCase() + "Controller";
            var serviceInterface = "I" + entityClassName+ "Service";
            var serviceVariable =  entityClassName.FirstCharToLowerCase() + "Service";
            var pkFieldsNames = Common.ConcatPkFieldTypes(table);

            string methodDeclarationGeneratedOutput = string.Empty;
            var methodDeclarationGenerated = GenerateClassMethods();
            foreach (var methodDeclaration in methodDeclarationGenerated)
            {
                if (methodDeclaration != String.Empty)
                    methodDeclarationGeneratedOutput += methodDeclaration + Environment.NewLine;
            }

            string output =
            $@"  
                [Authorize]
                [Route(""api/[controller]"")]
                [ApiController]
                public class {controllerName} : ControllerBase
                {{
                    private readonly ILogger<{controllerName}> logger;
                    private readonly {serviceInterface} {serviceVariable};

                    public {controllerName}({serviceInterface} {serviceVariable}, ILogger<{controllerName}> logger )
                    {{
                         this.{serviceVariable}={serviceVariable}; 
                         this.logger = logger;
                    }}
        
                    {methodDeclarationGeneratedOutput}

                }}
            ";

            return output;
        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateClassMethods()
        {
            var entityClassName = table.Name.Parts[1];
            bool isRefTable = refTables.Contains(entityClassName);

            yield return GenerateInsertDelegate(entityClassName);
            yield return GenerateUpdateDelegate(entityClassName);
            yield return GenerateDeleteDelegate(entityClassName);
            if (isRefTable) yield return GenerateGetAllRefDelegate(entityClassName); 
            yield return GenerateGetByPkDelegate(entityClassName); 
        }
         
        private string GenerateGetByPkDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetByPkFieldsNamesAsync]) return string.Empty;
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            var pkFieldsWithComma = Common.ConcatPkFieldsWithComma(table);
            string entityFirstLetterLower = entityClassName.FirstCharToLowerCase();
            string dtoClass = $"{entityClassName}ResponseGetByIdDto"; 
            string variableDtoClass = $"{entityFirstLetterLower}ResponseGetByIdDto";
            var serviceVariable = entityFirstLetterLower + "Service";

            string output = $@"
                /// <summary>
                /// Get {entityClassName} by Pk
                /// </summary>
                [HttpGet(""{{id}}"")]
                [ProducesResponseType(typeof({dtoClass}), (int)HttpStatusCode.OK)]
                [ProducesResponseType((int)HttpStatusCode.NotFound)]
                [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
                public async Task<IActionResult> GetBy{pkFieldsNames}Async({pkFieldsWithTypes})
                {{
                    {dtoClass}? {variableDtoClass}= null;
                    try
                    {{
                        var entity = await {serviceVariable}.GetBy{pkFieldsNames}Async({pkFieldsWithComma});
                        if(entity == null) 
                        {{
                            return StatusCode((int)HttpStatusCode.NotFound);
                        }}
                        {variableDtoClass} = entity.ToGetByIdDto();  
                    }}
                    catch(Exception ex)
                    {{
                        logger.LogError(ex, ""Problem in {entityClassName}Controller.GetBy{pkFieldsNames}Async method"");
                        return StatusCode((int)HttpStatusCode.InternalServerError, ""internal error "");
                    }} 
                    return Ok({variableDtoClass});
                }}" + Environment.NewLine;

            return output;
        }
         
        private string GenerateGetAllRefDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetAllAsync]) return string.Empty;
            string entityPascalCase = entityClassName.PascalCase();
            var serviceVariable = entityPascalCase + "Service";

            string output = $@"
                 /// <summary>
                 /// Get all {entityClassName} entities
                 /// </summary>
                 [HttpGet(""all"")]
                 [ProducesResponseType(typeof(IList<TranslatedItemDto>), (int)HttpStatusCode.OK)] 
                 [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
                 public async Task<IActionResult> GetAllAsync()
                 {{ 
                      IList<TranslatedItemDto>? enumItems = null;
                      try
                      {{
                        var entities = await translationService .GetTranslatedItems(""scope"");
                        if (entities != null && entities.Any())
                        {{
                           var translatedItems = entities.Select(x=> x.ToTranslatedItemDto());
                           enumItems = translatedItems.ToList();
                        }}
                    }}
                    catch (Exception ex)
                    {{
                         logger.LogError(ex, ""Problem in {entityPascalCase}Controller.Get method"");
                         return StatusCode((int)HttpStatusCode.InternalServerError, "" internal error "");
                    }}
                    return Ok(enumItems);
                 }}";

            return output;
        }
         
        private string GenerateInsertDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.InsertAsync]) return string.Empty;
            string dtoClass = $"{entityClassName}RequestAddDto";
            string entityFirstLetterLower = entityClassName.FirstCharToLowerCase();
            string variableDtoClass = $"{entityFirstLetterLower}RequestAddDto";
            var serviceVariable = entityFirstLetterLower + "Service";
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            string primaryKeyType = pkFieldsWithTypes.Split(' ')[0];

            return $@"
            /// <summary>
            /// Add new {entityClassName} entity
            /// </summary>
            [HttpPost(""add"")]
            [ProducesResponseType(typeof(long), (int)HttpStatusCode.OK)] 
            [ProducesResponseType((int)HttpStatusCode.InternalServerError)] 
            public async Task<IActionResult> AddAsync({dtoClass} {variableDtoClass})
            {{
                {primaryKeyType} id = 0;
                try
                {{
                     var entity = {variableDtoClass}.ToEntity();
                     await {serviceVariable}.InsertAsync(entity);
                     id = entity.Id; 
                }}
                catch(Exception ex)
                {{
                    logger.LogError(ex, ""Problem in {entityFirstLetterLower}Controller.Add method"");
                    return StatusCode((int)HttpStatusCode.InternalServerError, "" internal error "");
                }} 
                return Ok(id);
            }}";
        }

        private string GenerateUpdateDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.UpdateAsync]) return string.Empty;
            string dtoClass = $"{entityClassName}RequestUpdateDto";
            string entityFirstLetterLower = entityClassName.FirstCharToLowerCase();
            string variableDtoClass = $"{entityFirstLetterLower}RequestUpdateDto";
            var serviceVariable = entityFirstLetterLower + "Service";

            string output = $@"
            /// <summary>
            /// Update {entityClassName} entity
            /// </summary>
             [HttpPost(""udpate"")]
            [ProducesResponseType((int)HttpStatusCode.OK)]
            [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
            public async Task<IActionResult> UpdateAsync({dtoClass} {variableDtoClass})
            {{
                try
                {{
                   var {entityFirstLetterLower} = await {serviceVariable}.GetByIdAsync({variableDtoClass}.Id);
                   if ({entityFirstLetterLower} == null)
                   {{
                        return StatusCode((int)HttpStatusCode.NotFound);
                   }}
                   await {serviceVariable}.UpdateAsync({entityFirstLetterLower});
                }}
                catch(Exception ex)
                {{
                    logger.LogError(ex, ""Problem in {entityFirstLetterLower}Controller.Update method"");
                    return StatusCode((int)HttpStatusCode.InternalServerError, ""internal error "");
                }}
                return Ok();
            }}" + Environment.NewLine;
            return output;
        }

        private string GenerateDeleteDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.DeleteAsync]) return string.Empty;
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            var pkFieldsWithComma = Common.ConcatPkFieldsWithComma(table);
            string entityFirstLetterLower = entityClassName.FirstCharToLowerCase();
            var serviceVariable = entityFirstLetterLower + "Service";

            string output = $@"
                        /// <summary>
                        /// Delete {entityClassName} entity
                        /// </summary>
                         [HttpDelete(""delete/{{id}}"")]
                        [ProducesResponseType((int)HttpStatusCode.OK)]
                        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
                        public async Task<IActionResult> DeleteAsync({pkFieldsWithTypes})
                        {{
                            try
                            {{
                                 var {entityFirstLetterLower} = await {serviceVariable}.GetByIdAsync({pkFieldsWithComma});
                                 if ({entityFirstLetterLower} == null)
                                 {{
                                        return StatusCode((int)HttpStatusCode.NotFound);
                                 }}
                                 await {serviceVariable}.DeleteAsync({entityFirstLetterLower});
                            }}
                            catch(Exception ex)
                            {{
                                 logger.LogError(ex, ""Problem in {entityFirstLetterLower}Controller.Update method"");
                                 return StatusCode((int)HttpStatusCode.InternalServerError, ""internal error "");
                            }}
                            return Ok();
                        }}" + Environment.NewLine;
            return output;
        }


    }
}
