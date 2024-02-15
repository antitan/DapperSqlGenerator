using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Factory;
using DapperSqlGenerator.App.Models;
using DapperSqlGenerator.App.Services;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DapperSqlGenerator.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string projectName = "SmartCv";
            string connectionString = "Data Source=localhost;Initial Catalog=SmartCv;Integrated Security=True;Persist Security Info=False;Trust Server Certificate=True;";
             
            //directories where files are created
            string dataModelDir = @"C:\Temp\Data";
            if (!Directory.Exists(dataModelDir)) Directory.CreateDirectory(dataModelDir);
            string dataRepositoryDir = @"C:\Temp\Repositories";
            if (!Directory.Exists(dataRepositoryDir)) Directory.CreateDirectory(dataRepositoryDir);
            string dataServiceDir = @"C:\Temp\Services";
            if (!Directory.Exists(dataServiceDir)) Directory.CreateDirectory(dataServiceDir);
            string cacheServiceDir = @"C:\Temp\Cache";
            if (!Directory.Exists(cacheServiceDir)) Directory.CreateDirectory(cacheServiceDir);
            string configurationDir = @"C:\Temp\Configuration";
            if (!Directory.Exists(configurationDir)) Directory.CreateDirectory(configurationDir);
            string registerServiceExtensionDir = @"C:\Temp\Extensions";
            if (!Directory.Exists(registerServiceExtensionDir)) Directory.CreateDirectory(registerServiceExtensionDir);
            string helpersDir = @"C:\Temp\Helpers";
            if (!Directory.Exists(helpersDir)) Directory.CreateDirectory(helpersDir);
            string constantsDir = @"C:\Temp\Constants";
            if (!Directory.Exists(constantsDir)) Directory.CreateDirectory(constantsDir);
            string spDir = @"C:\Temp\StoredProcedures";
            if (!Directory.Exists(spDir)) Directory.CreateDirectory(spDir);


            spDir = constantsDir = helpersDir = registerServiceExtensionDir = configurationDir = cacheServiceDir = dataServiceDir = dataRepositoryDir = dataModelDir = @"C:\proj_net\testGenerator\ConsoleApp1";

            //Choice of methods to generate
            MethodsToGenerate.Check = new Dictionary<string,bool>()
            {
                {MethodNameToGenerate.GetAllAsync, true},
                {MethodNameToGenerate.GetPaginatedAsync, true},
                {MethodNameToGenerate.GetByPkFieldsNamesAsync, true},
                {MethodNameToGenerate.GetByExpressionAsync, true},
                {MethodNameToGenerate.InsertAsync, true},
                {MethodNameToGenerate.UpdateAsync, true},
                {MethodNameToGenerate.DeleteByPkFieldsNamesAsync, true},
                {MethodNameToGenerate.DeleteByExpressionAsync, true}
            };

            //if includeOnlyTables is not empty , let excludedTables empty it will be computed afted
            //else if includeOnlyTables is empty , you can fill excludedTables
            string[] includeOnlyTables = { };
            //excludes table we don't want ot generate
            //string[] excludedTables = {  };
            string[] excludedTables = { "EFMigrationsHistory", "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins", "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens" };

            //references table (static tables)
            //For these tables, GetAllAsync is genereated and cache is used inside the service
            string[] refTables = { "Certification", "CountryCompany", "Department", "Lang" , "JobOfferLevel" };

            string dataModelNamespace       = $"{projectName}.Model";
            string dataRepostioryNamespace  = $"{projectName}.Repositories";
            string dataServiceNamespace     = $"{projectName}.Services";

            var model = TSqlModelFactory.CreateModel(connectionString);

            if (includeOnlyTables.Any())
            {
                var  alltables = model.GetAllTables().Select(t => t.Name.Parts[1].PascalCase()).ToArray();
                excludedTables = alltables.Except(includeOnlyTables).ToArray();
            }
             

            List<IGeneratorService> generatorServices = new List<IGeneratorService>();
            //generate model classes
            generatorServices.Add(new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables));
            //generate repository layer classes
            generatorServices.Add(new RepositoryGeneratorService(dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, projectName, excludedTables));
            //generate services layer classes
            generatorServices.Add(new ServicesGeneratorService(dataServiceNamespace, dataModelNamespace, dataRepostioryNamespace, dataServiceDir, projectName, excludedTables, refTables));
            //copy files utils
            generatorServices.Add(new CopyUtilitiesFilesService(projectName, cacheServiceDir, configurationDir, helpersDir));
            //generate custom files
            generatorServices.Add(new FileCustomerService(projectName, registerServiceExtensionDir, constantsDir, excludedTables, refTables));
            //generate stored procedure calls
            generatorServices.Add(new StoredProcedureGeneratorService(projectName, dataRepostioryNamespace, spDir));

            //Debug
            //generate model classes
            //await new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables).GenerateFilesAsync(model);
            //generate repository layer classes
            //await new RepositoryGeneratorService(dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, projectName,excludedTables).GenerateFilesAsync(model);
            //generate services layer classes
            //await new ServicesGeneratorService(dataServiceNamespace, dataModelNamespace, dataRepostioryNamespace, dataServiceDir, projectName, excludedTables, refTables).GenerateFilesAsync(model);
            //copy files utils
            //await new CopyUtilitiesFilesService(projectName, cacheServiceDir, configurationDir, helpersDir).GenerateFilesAsync(model);
            //generate custom files
            //await new FileCustomerService(projectName, registerServiceExtensionDir, constantsDir, excludedTables, refTables).GenerateFilesAsync(model);
            //generate stored procedure calls
            //await new StoredProcedureGeneratorService(projectName, dataRepostioryNamespace, spDir).GenerateFilesAsync(model);

            List<Task> tasks = new List<Task>();
            generatorServices.ForEach(serv =>
            {
                tasks.Add(Task.Run(() => serv.GenerateFilesAsync(model)));
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);

            var warnings = generatorServices.SelectMany(s => s.Warnings);
            bool hasWarning = warnings.Any();
            if(hasWarning)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"--SOME WARNINGS--");
            }
            foreach (var warning in warnings)
            {
                //Console.WriteLine(warning);
                System.Console.WriteLine(warning);  
            } 
            if (hasWarning)
            { 
                System.Console.ReadLine();
            }
             
        }
    }
}
