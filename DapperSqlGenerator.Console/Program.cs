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
            string projectName = "MyProject";
            string connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=SmartCv;Integrated Security=True;Persist Security Info=False;Trust Server Certificate=True;";

            string csProjPath = @"C:\proj_net\testGenerator\ConsoleApp1";

            //directories where files are created
            string dataModelDir = Path.Combine(csProjPath, $"{projectName}.Business" ,"DataModel");
            if (!Directory.Exists(dataModelDir)) 
                Directory.CreateDirectory(dataModelDir);

            string cacheServiceDir = Path.Combine(csProjPath, $"{projectName}.Common", "Cache");
            if (!Directory.Exists(cacheServiceDir))
                Directory.CreateDirectory(cacheServiceDir);

            string configurationDir = Path.Combine(csProjPath, $"{projectName}.Common", "Configuration");
            if (!Directory.Exists(configurationDir))
                Directory.CreateDirectory(configurationDir);

            string helpersDir = Path.Combine(csProjPath, $"{projectName}.Common", "Helpers");
            if (!Directory.Exists(helpersDir))
                Directory.CreateDirectory(helpersDir);

            string constantsDir = Path.Combine(csProjPath, $"{projectName}.Common", "Constants");
            if (!Directory.Exists(constantsDir))
                Directory.CreateDirectory(constantsDir);


            string dataRepositoryDir = Path.Combine(csProjPath, $"{projectName}.Repositories");  
            if (!Directory.Exists(dataRepositoryDir)) 
                Directory.CreateDirectory(dataRepositoryDir);
            
            string dataServiceDir = Path.Combine(csProjPath, $"{projectName}.Services"); 
            if (!Directory.Exists(dataServiceDir)) 
                Directory.CreateDirectory(dataServiceDir);

            
            string registerServiceExtensionDir = Path.Combine(csProjPath, "Extensions");  
            if (!Directory.Exists(registerServiceExtensionDir)) 
                Directory.CreateDirectory(registerServiceExtensionDir);
            
          
            
            string spDir = Path.Combine(csProjPath, $"{projectName}.Repositories","StoredProcedures"); 
            if (!Directory.Exists(spDir)) 
                Directory.CreateDirectory(spDir);

            //Choice of methods to generate
            MethodsToGenerate.Check = new Dictionary<string,bool>()
            {
                {MethodNameToGenerate.GetAllAsync, true},
                {MethodNameToGenerate.GetPaginatedAsync, true},
                {MethodNameToGenerate.GetByPkFieldsNamesAsync, true},
                {MethodNameToGenerate.GetByExpressionAsync, true},
                {MethodNameToGenerate.DeleteByExpressionAsync, true},

                {MethodNameToGenerate.InsertAsync, true},
                {MethodNameToGenerate.UpdateAsync, true},
                {MethodNameToGenerate.DeleteByPkFieldsNamesAsync, true},
               
                //method if you need to operate inside transaction
                {MethodNameToGenerate.InsertAsyncTransaction, true},
                {MethodNameToGenerate.UpdateAsyncTransaction, true},
                {MethodNameToGenerate.DeleteByPkFieldsNamesAsyncTransaction, true}
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

            string dataModelNamespace       = $"{projectName}.Business.DataModel";
            string dataRepostioryNamespace  = $"{projectName}.Repositories";
            string dataServiceNamespace     = $"{projectName}.Services";

            var model = TSqlModelFactory.CreateModel(connectionString);

            if (includeOnlyTables.Any())
            {
                var  alltables = model.GetAllTables().Select(t => t.Name.Parts[1].PascalCase()).ToArray();
                excludedTables = alltables.Except(includeOnlyTables).ToArray();
            }

            bool splitInterfacesAndClassesFile = true;
             
            List<IGeneratorService> generatorServices = new List<IGeneratorService>();
            //generate model classes
            generatorServices.Add(new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables));
            //generate repository layer classes
            generatorServices.Add(new RepositoryGeneratorService(splitInterfacesAndClassesFile,dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, projectName, excludedTables));
            //generate services layer classes
            generatorServices.Add(new ServicesGeneratorService(splitInterfacesAndClassesFile,dataServiceNamespace, dataModelNamespace, dataRepostioryNamespace, dataServiceDir, projectName, excludedTables, refTables));
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
