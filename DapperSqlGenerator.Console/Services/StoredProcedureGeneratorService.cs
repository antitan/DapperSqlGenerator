using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Services
{
    public class StoredProcedureGeneratorService : IGeneratorService
    {
        string projectName;
        string dataModelNamespace;
        string serviceModelNamespace;
        string dataRepositoryNamespace;
        string dirToWrite;
        string[] excludedTables;
        string[] refTables;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public StoredProcedureGeneratorService(string serviceModelNamespace, string dataModelNamespace, string dataRepositoryNamespace, string dirToWrite, string projectName, string[] excludedTables, string[] refTables)
        {
            this.serviceModelNamespace = serviceModelNamespace;
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.refTables = refTables;
            this.dataModelNamespace = dataModelNamespace;
            this.dataRepositoryNamespace = dataRepositoryNamespace;
            this.projectName = projectName;
        }

        public async Task GenerateFilesAsync(TSqlModel model)
        {
            //stored Procedures
            var objs = model.GetObjects(DacQueryScopes.All, Procedure.TypeClass).ToList();
            foreach (var proc in objs)
            {
                var name = proc.Name;
                foreach (var parameter in proc.GetChildren().Where(child => child.ObjectType.Name == "Parameter"))
                {
                    var dataType = parameter.GetReferenced(Parameter.DataType).First(); // Obtiene el tipo del parametro.
                    var parameterName = parameter.Name.Parts.Last();
                }
            }
            //stored Procedures
        }



    }
}
