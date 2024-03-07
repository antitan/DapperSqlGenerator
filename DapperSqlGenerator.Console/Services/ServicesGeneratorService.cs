using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator.Services;
using DapperSqlGenerator.App.Helpers; 
using Microsoft.SqlServer.Dac.Model; 
using System.Text; 

namespace DapperSqlGenerator.App.Services
{ 
    public class ServicesGeneratorService : IGeneratorService
    {
        string projectName;
        string dataModelNamespace;
        string serviceModelNamespace;
        string dataRepositoryNamespace;
        string dirToWrite;
        string[] excludedTables;
        string[] refTables;
        bool splitInterfacesAndClassesFile;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public ServicesGeneratorService(bool splitInterfacesAndClassesFile,string serviceModelNamespace, string dataModelNamespace, string dataRepositoryNamespace, string dirToWrite, string projectName, string[] excludedTables, string[] refTables)
        {
            this.splitInterfacesAndClassesFile = splitInterfacesAndClassesFile;
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
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    var serviceGenerator = new ServicesGenerator(serviceModelNamespace, dataModelNamespace, dataRepositoryNamespace, projectName, refTables, table);
                    
                    string serviceClassFilePath = Path.Combine(dirToWrite, $"{entityName}Service.cs");
                    if (File.Exists(serviceClassFilePath)) 
                        File.Delete(serviceClassFilePath);

                    if (splitInterfacesAndClassesFile)
                    {
                        //generate interface
                        string interfaceClassFilePath = Path.Combine(dirToWrite, $"I{entityName}Service.cs");
                        if (File.Exists(interfaceClassFilePath))
                            File.Delete(interfaceClassFilePath);

                        using (var fileStream = File.Open(interfaceClassFilePath, FileMode.Create, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                string cs = serviceGenerator.GenerateInterfacePart();
                                if (cs != string.Empty)
                                {
                                    string formatedCode = CodeFormatterHelper.ReformatCode(cs);
                                    await writer.WriteLineAsync(formatedCode);
                                }
                            }
                        }
                        //generate class
                        using (var fileStream = File.Open(serviceClassFilePath, FileMode.Create, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                string cs = serviceGenerator.GenerateClassPart();
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
}
