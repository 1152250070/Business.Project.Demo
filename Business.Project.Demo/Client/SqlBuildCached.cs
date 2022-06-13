using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Project.Demo.Client
{
    public class SqlBuildCached
    {
        Dictionary<string, string> _cachedOfModules = new Dictionary<string, string>();

        public static SqlBuildCached Create()
        {
            var instance = new SqlBuildCached();
            return instance;
        }

        public string GetDateTime(string modeluCode)
        {
            if (_cachedOfModules.ContainsKey(modeluCode))
            {
                return _cachedOfModules[modeluCode];
            }
            string datestr = DateTime.Now.ToString();
            _cachedOfModules.Add(modeluCode, datestr);
            return datestr;
        }
    }

    public class FormSqlData
    {
        public static void Get(SqlBuildCached sqlBuildCached = null)
        {
            sqlBuildCached = sqlBuildCached ?? SqlBuildCached.Create();
            string str = sqlBuildCached.GetDateTime("01");
        }
    }
}
