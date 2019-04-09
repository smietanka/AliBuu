using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliBuu.DataAccess.Helpers
{
    static class SqlPartHelper
    {
        public static string GenerateStringTable(string tableName, List<string> param)
        {
            if(param.Any())
            {
                var sb = new StringBuilder();
                var maxLengthParam = param.Max(q => q.Length);
                sb.AppendLine($"declare @{tableName} table(id varchar({maxLengthParam}));");

                var values = string.Join(", ", param.Select(q => string.Format($"('{q.Replace("'", "''").Trim(':', '\0')}')")));

                sb.AppendLine($"insert into @{tableName} (id) values {values};");

                return sb.ToString();
            }
            return string.Empty;
        }

        public static string GenerateIntStringTable(string tableName, List<KeyValuePair<int, string>> param)
        {
            if (param.Any())
            {
                var sb = new StringBuilder();
                var maxLengthParam = param.Max(q => q.Value.Length);
                sb.AppendLine($"declare @{tableName} table(tabKey int, tabValue varchar({maxLengthParam}));");

                var values = string.Join(", ", param.Select(q => string.Format($"({q.Key}, '{q.Value.Replace("'", "''").Trim(':', '\0')}')")));

                sb.AppendLine($"insert into @{tableName} (tabKey, tabValue) values {values};");

                return sb.ToString();
            }
            return string.Empty;
        }

        public static string GenerateIntStringTable(string tableName, Dictionary<int, string> param)
        {
            if (param.Any())
            {
                var sb = new StringBuilder();
                var maxLengthParam = param.Max(q => q.Value.Length);
                sb.AppendLine($"declare @{tableName} table(tabKey int, tabValue varchar({maxLengthParam}));");

                var values = string.Join(", ", param.Select(q => string.Format($"({q.Key}, '{q.Value.Replace("'", "''").Trim(':', '\0')}')")));

                sb.AppendLine($"insert into @{tableName} (tabKey, tabValue) values {values};");

                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
