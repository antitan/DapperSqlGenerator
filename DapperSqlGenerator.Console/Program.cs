using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Factory;
using DapperSqlGenerator.App.Services;

namespace DapperSqlGenerator.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string projectName = "MyProject";
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

            //if includeOnlyTables is not empty , let excludedTables empty it will be computed afted
            //else if includeOnlyTables is empty , you can fill excludedTables
            string[] includeOnlyTables = { "CvSkillExperienceMapping" };
            //excludes table we don't want ot generate
            //string[] excludedTables = {  };
            string[] excludedTables = { "EFMigrationsHistory", "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins", "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens" };

            //references table (static tables)
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
            generatorServices.Add(new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables));
            generatorServices.Add(new RepositoryGeneratorService(dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, projectName, excludedTables));
            generatorServices.Add(new ServicesGeneratorService(dataServiceNamespace, dataModelNamespace, dataRepostioryNamespace, dataServiceDir, projectName, excludedTables, refTables));
            generatorServices.Add(new CopyUtilitiesFilesService(projectName, cacheServiceDir, configurationDir, helpersDir));
            generatorServices.Add(new FileCustomerService(projectName, registerServiceExtensionDir, constantsDir, excludedTables, refTables));
            generatorServices.Add(new StoredProcedureGeneratorService(projectName, dataRepostioryNamespace, spDir));

            //await new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables).GenerateFilesAsync(model);
            //await new RepositoryGeneratorService(dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, projectName,excludedTables).GenerateFilesAsync(model);
            //await new ServicesGeneratorService(dataServiceNamespace, dataModelNamespace, dataRepostioryNamespace, dataServiceDir, projectName, excludedTables, refTables).GenerateFilesAsync(model);
            //await new CopyUtilitiesFilesService(projectName, cacheServiceDir, configurationDir, helpersDir).GenerateFilesAsync(model);
            //await new FileCustomerService(projectName, registerServiceExtensionDir, constantsDir, excludedTables, refTables).GenerateFilesAsync(model);
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
