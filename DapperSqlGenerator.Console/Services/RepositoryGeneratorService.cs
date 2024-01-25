
using DapperSqlGenerator.App.Exceptions;
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator.Repositories;
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

        public List<string> Warnings
        {
            get
            {
                return warnings;
            }
        }

        public RepositoryGeneratorService(string datamodelNamespace,  string dataRepostioryNamespace, string dirToWrite, string projectName,string[] excludedTables)
        { 
            this.datamodelNamespace= datamodelNamespace;
            this.dataRepostioryNamespace = dataRepostioryNamespace;
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.projectName = projectName;
            warnings = new List<string>();
        }



        private string ExtractGeneratedRegion(string csCode)
        {
            string regionContent = string.Empty;
            string pattern = @"#region Generated(.*?)#endregion Generated";

            // Recherche du contenu de la région Generated
            Match match = Regex.Match(csCode, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                // Extraction du contenu de la région Generated
                 regionContent = match.Groups[1].Value;
            }
            else
            {
                throw new RegionGeneratedNotFoundException();
            }

            return regionContent;   
        }

        private void ReplaceGeneratedRegion(string existingCs,string extractedRegionContent, string filePathToReplace)
        {
            string pattern = @"#region Generated.*?#endregion Generated";
            string newContentToInsert = $"#region Generated\n\n {extractedRegionContent}\n\n#endregion Generated";
            string newFileContent = Regex.Replace(existingCs, pattern, newContentToInsert, RegexOptions.Singleline);
            string formatedCode = CodeFormatterHelper.ReformatCode(newFileContent);
            File.WriteAllText(filePathToReplace, formatedCode);
        }
         

        public async Task GenerateFilesAsync(TSqlModel model)
        { 
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    string filePath = Path.Combine(dirToWrite, $"{entityName}Repository.cs");
                    bool hasWarning = false;
                    if (!File.Exists(filePath))
                    {
                        using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
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
                    else
                    {
                        string existingCs = File.ReadAllText(filePath);
                        string newCs = new DapperRepositoryClassGenerator(projectName, datamodelNamespace, dataRepostioryNamespace, table).Generate();

                        string extractedRegionContentFromNewFile = string.Empty;
                        try
                        {
                            extractedRegionContentFromNewFile = ExtractGeneratedRegion(newCs);
                        }
                        catch(RegionGeneratedNotFoundException ex)
                        {
                            warnings.Add(" Problem to extract Generated Section from generated Code, Generated Section not found");
                            hasWarning = true;
                        }
                        string extractedRegionContentFromExstingFile = string.Empty;
                        try
                        {
                            extractedRegionContentFromExstingFile = ExtractGeneratedRegion(existingCs);
                        }
                        catch (RegionGeneratedNotFoundException ex)
                        {
                            warnings.Add($" Problem to extract Generated Section from existing File {filePath}, Generated Section not found ");
                            hasWarning = true;
                        }
                        if (!hasWarning)
                        {
                            //implement a guard
                            var extractedNumberMethodsFromNewFile = ClassHelper.CountOccurrences(extractedRegionContentFromNewFile,"public");
                            var extractedNumberMethodsFromExistingFile = ClassHelper.CountOccurrences(extractedRegionContentFromExstingFile, "public");
                            if(extractedNumberMethodsFromNewFile < extractedNumberMethodsFromExistingFile)
                            {
                                warnings.Add($" File {filePath} will be not updated because you wrote some methods inside Generated Region, move no generated methods out this region");
                            }
                            else
                                ReplaceGeneratedRegion(existingCs, extractedRegionContentFromNewFile, filePath);
                        }
                    }
                }
            }
        }
    }
}
