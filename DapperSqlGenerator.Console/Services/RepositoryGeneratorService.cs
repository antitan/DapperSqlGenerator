
using DapperSqlGenerator.App.Exceptions;
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator.Repositories;
using DapperSqlGenerator.App.Generator.Services;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperSqlGenerator.App.Services
{
    public class RepositoryGeneratorService : IGeneratorService
    {
        string datamodelNamespace;
        string dataRepostioryNamespace;
        string dirToWrite;
        string[] excludedTables;
        string projectName;
        List<string> warnings;
        bool splitInterfacesAndClassesFile;

        public List<string> Warnings
        {
            get
            {
                return warnings;
            }
        }

        public RepositoryGeneratorService(bool splitInterfacesAndClassesFile,string datamodelNamespace,  string dataRepostioryNamespace, string dirToWrite, string projectName,string[] excludedTables)
        { 
            this.splitInterfacesAndClassesFile= splitInterfacesAndClassesFile;
            this.datamodelNamespace= datamodelNamespace;
            this.dataRepostioryNamespace = dataRepostioryNamespace;
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.projectName = projectName;
            warnings = new List<string>();
        }         

        public async Task GenerateFilesAsync(TSqlModel model)
        { 
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    var repositoryGenerator = new DapperRepositoryClassGenerator(projectName, datamodelNamespace, dataRepostioryNamespace, table);

                    string repositoryClassFilePath = Path.Combine(dirToWrite, $"{entityName}Repository.cs");
                    if (File.Exists(repositoryClassFilePath))
                        File.Delete(repositoryClassFilePath);

                    if (splitInterfacesAndClassesFile)
                    {
                        //generate interface
                        string interfaceClassFilePath = Path.Combine(dirToWrite, $"I{entityName}Repository.cs");
                        if (File.Exists(interfaceClassFilePath))
                            File.Delete(interfaceClassFilePath);

                        using (var fileStream = File.Open(interfaceClassFilePath, FileMode.Create, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                string cs = repositoryGenerator.GenerateInterfacePart();
                                if (cs != string.Empty)
                                {
                                    string formatedCode = CodeFormatterHelper.ReformatCode(cs);
                                    await writer.WriteLineAsync(formatedCode);
                                }
                            }
                        }
                        //generate class
                        using (var fileStream = File.Open(repositoryClassFilePath, FileMode.Create, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                string cs = repositoryGenerator.GenerateClassPart();
                                if (cs != string.Empty)
                                {
                                    string formatedCode = CodeFormatterHelper.ReformatCode(cs);
                                    await writer.WriteLineAsync(formatedCode);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var fileStream = File.Open(repositoryClassFilePath, FileMode.Create, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                string cs = new DapperRepositoryClassGenerator(projectName, datamodelNamespace, dataRepostioryNamespace, table).Generate();
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
}
