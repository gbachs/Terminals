using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace Terminals.Network.WMI
{
    internal class PivotDataTable
    {
        public static Dictionary<string, string> ConvertToNameValue(DataTable dataValues, int index)
        {
            var nv = new Dictionary<string, string>();

            var row = dataValues.Rows[index];
            //columns become the names
            foreach (DataColumn col in dataValues.Columns)
                nv.Add(col.ColumnName, row[col].ToString());

            return nv;
        }

        private static void AddPropertyAndField(CodeTypeDeclaration classDec, Type DataType, string DataTypeString,
            string Name)
        {
            CodeMemberField field = null;
            if (DataType != null)
                field = new CodeMemberField(DataType, Name + "_field");
            else
                field = new CodeMemberField(DataTypeString, Name + "_field");

            classDec.Members.Add(field);
            var prop = new CodeMemberProperty();

            if (DataType != null)
                prop.Type = new CodeTypeReference(DataType);
            else
                prop.Type = new CodeTypeReference(DataTypeString);

            prop.Name = Name;
            prop.HasGet = true;
            prop.HasSet = false;
            prop.Attributes = MemberAttributes.Public;
            prop.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                    field.Name)));
            classDec.Members.Add(prop);
        }

        public static Assembly CreateAssemblyFromDataTable(DataTable DataValues)
        {
            var rnd = new Random();
            if (DataValues.TableName == null || DataValues.TableName == string.Empty)
                DataValues.TableName = rnd.Next().ToString();

            var classDec = new CodeTypeDeclaration(DataValues.TableName);
            classDec.IsClass = true;

            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            classDec.Members.Add(classDec);

            foreach (DataColumn col in DataValues.Columns)
                AddPropertyAndField(classDec, col.DataType, string.Empty, col.ColumnName);

            AddPropertyAndField(classDec, null, "System.Collections.Generic.List<" + DataValues.TableName + ">",
                "ListOf" + DataValues.TableName);

            using (var provider = new CSharpCodeProvider())
            {
                //ICodeGenerator generator = provider.CreateGenerator();

                var ns = new CodeNamespace("Terminals.Generated");
                ns.Types.Add(classDec);
                var options = new CodeGeneratorOptions();
                //options.BlankLinesBetweenMembers = true;
                var filename = Path.GetTempFileName();
                using (var sw = new StreamWriter(filename, false))
                {
                    //generator.GenerateCodeFromNamespace(ns, sw, options);
                    provider.GenerateCodeFromNamespace(ns, sw, options);

                    //ICodeCompiler icc = provider.CreateCompiler();

                    var compileParams = new CompilerParameters();
                    compileParams.GenerateExecutable = false;
                    compileParams.GenerateInMemory = true;

                    //return icc.CompileAssemblyFromSource(compileParams, System.IO.File.ReadAllText(filename)).CompiledAssembly;
                    var icc = provider.CompileAssemblyFromSource(compileParams, File.ReadAllText(filename));
                    return icc.CompiledAssembly;
                }
            }
        }

        public static object CreateTypeFromDataTable(DataTable DataValues)
        {
            var asm = CreateAssemblyFromDataTable(DataValues);
            var instance = asm.CreateInstance(DataValues.TableName);
            return null;
        }
    }
}