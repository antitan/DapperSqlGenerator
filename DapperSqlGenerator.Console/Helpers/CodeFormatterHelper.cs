using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace DapperSqlGenerator.Console.Helpers
{
    public class CodeFormatterHelper
    {
        public static string ReformatCode(string code)
        {
            // Parse le code source
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // Mettre en forme le code
            var formattedRoot = Formatter.Format(root, new AdhocWorkspace());
            var formattedCode = formattedRoot.ToFullString();

            return formattedCode;
        }
    }
}
