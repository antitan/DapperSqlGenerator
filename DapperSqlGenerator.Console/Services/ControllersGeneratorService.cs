using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator.Controllers;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.App.Services
{
    public class ControllersGeneratorService : IGeneratorService
    {
        string dirToWrite;
        string[] excludedTables;
        string[] refTables;
        string serviceNamespace;
        string controllerNamespace;
        string mapperNamespace;
        string requestNamespace;
        string responseNamespace;
        string modelNamespace;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public ControllersGeneratorService(string modelNamespace, string controllerNamespace, string mapperNamespace, string requestNamespace, string responseNamespace, string serviceNamespace, string dirToWrite, string[] excludedTables, string[] refTables)
        {
            this.modelNamespace = modelNamespace;
            this.controllerNamespace = controllerNamespace;
            this.mapperNamespace = mapperNamespace;
            this.serviceNamespace = serviceNamespace;
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.refTables = refTables;
            this.requestNamespace = requestNamespace;
            this.responseNamespace = responseNamespace;

        }

        public async Task GenerateFilesAsync(TSqlModel model)
        {
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    var serviceGenerator = new ControllerGenerator(modelNamespace, controllerNamespace, mapperNamespace, requestNamespace, responseNamespace, serviceNamespace, refTables, table);

                    string serviceClassFilePath = Path.Combine(dirToWrite, $"{entityName}Controller.cs");
                    if (File.Exists(serviceClassFilePath))
                        File.Delete(serviceClassFilePath);

                    using (var fileStream = File.Open(serviceClassFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            string cs = serviceGenerator.Generate();
                            if (cs != string.Empty)
                            {
                                string formatedCode = CodeFormatterHelper.ReformatCode(cs);
                                await writer.WriteLineAsync(formatedCode);
                            }
                        }
                    }
                }
            }
        }
    }
}
