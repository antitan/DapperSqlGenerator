using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.Console.Services
{
    public interface IGeneratorService
    {
        Task GenerateFilesAsync(TSqlModel model);
    }
}
