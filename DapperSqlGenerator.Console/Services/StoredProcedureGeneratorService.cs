using DapperSqlGenerator.App.Generator.Repositories;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System.Text;

namespace DapperSqlGenerator.App.Services
{
    public class StoredProcedureGeneratorService : IGeneratorService
    { 
        string dataRepositoryNamespace;
        string dirToWrite;
        string projectName;
        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public StoredProcedureGeneratorService(string projectName, string dataRepositoryNamespace, string dirToWrite)
        { 
            this.dirToWrite = dirToWrite; 
            this.dataRepositoryNamespace = dataRepositoryNamespace;
            this.projectName = projectName;
        }

        public async Task GenerateFilesAsync(TSqlModel model)
        {
            //stored Procedures
            var objs = model.GetObjects(DacQueryScopes.All, Procedure.TypeClass).ToList();
            foreach (var proc in objs)
            {
                Console.WriteLine("Create StoredProcedure Access " + proc.Name);
                var storedProcedureName = proc.Name.Parts[1].Replace("[", string.Empty).Replace("]", string.Empty);
                Dictionary<string, string> paramNamesTypes = new Dictionary<string, string>();
                foreach (var parameter in proc.GetChildren().Where(child => child.ObjectType.Name == "Parameter"))
                {
                    var dataType = parameter.GetReferenced(Parameter.DataType).First().Name; // Obtiene el tipo del parametro.
                    var parameterName = parameter.Name.Parts.Last();
                    paramNamesTypes.Add(parameterName.Replace("@", ""), dataType.ToString().Replace("[",string.Empty).Replace("]", string.Empty));
                }

                string filePath = Path.Combine(dirToWrite, $"{storedProcedureName}.cs");
                using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        string cs = new StoredProcedureFileGenerator(projectName, dataRepositoryNamespace, storedProcedureName, paramNamesTypes).Generate();
                        if (cs != string.Empty)
                        {
                            string formatedCode = CodeFormatterHelper.ReformatCode(cs);
                            await writer.WriteLineAsync(formatedCode);
                        }
                    }
                }
            }
            //stored Procedures
        }



    }
}
