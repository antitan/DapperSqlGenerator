using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Factory
{
    public class TSqlModelFactory
    {
        public static TSqlModel CreateModel(string connexionString)
        {
            ModelExtractOptions options = new ModelExtractOptions()
            {
                LoadAsScriptBackedModel = false,
            };
            return TSqlModel.LoadFromDatabase(connexionString, options);
            //ModelLoadOptions options = new ModelLoadOptions()
            //{
            //    LoadAsScriptBackedModel = false,
            //};
            //return TSqlModel.LoadFromDacpac(dacpacFileName, options);
        }
    }
}
