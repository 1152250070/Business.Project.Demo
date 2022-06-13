using Business.Project.Demo.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Project.Demo.Client
{
    public class AdressIdentify
    {
        public static void GetAdress()
        {
            string content = FileUtil.GetContent();
            var areaList = JsonConvert.DeserializeObject<List<tbmain_area>>(content) ?? new List<tbmain_area>();
            DataTable table = new AdressIdentify().CreateTable(areaList);
            AddressToCityIDs addressModel = new AddressToCityIDs(table);
            string address = "江苏南京浦口江浦街道";//"江苏南京浦口区江浦街道";//杭州舟山东路
            string cityf_name = addressModel.GetCityName(address);
        }

        private DataTable CreateTable(List<tbmain_area> areaList)
        {
            DataTable table = new DataTable();
            table.Columns.Add("f_code");
            table.Columns.Add("f_parentcode");
            table.Columns.Add("f_name");
            table.Columns.Add("f_type", typeof(int));
            foreach (tbmain_area area in areaList)
            {
                var row = table.NewRow();
                row["f_code"] = area.code;
                row["f_parentcode"] = area.parentcode;
                row["f_name"] = area.name;
                row["f_type"] = area.type;
                table.Rows.Add(row);
            }
            return table;
        }
    }
    /// <summary>
    /// C# 地址分析算法，返回省市区行政区域编码
    /// </summary>
    public class AddressToCityIDs
    {
        private DataTable table;

        public AddressToCityIDs(DataTable dtCitys)
        {
            this.table = dtCitys;
        }


        public string GetCityName(string address)
        {
            List<string> codeList = GetCityAddress(address);
            string cityNames = GetCityName(codeList);//上海市-市辖区-金山区
            return cityNames;
        }

        public List<string> GetCityAddress(string address)
        {
            List<string> codeList = new List<string>();
            table.DefaultView.Sort = "f_type ASC";
            //枚举所有城市名称，匹配地址
            foreach (DataRow row in table.DefaultView.ToTable().Rows)
            {
                var f_name = row["f_name"].ToString();
                int f_type = Convert.ToInt32(row["f_type"]);
                //上海市宝山区丰翔路888号
                if (!MatchName(address, f_name, f_type))
                    continue;

                string f_code = row["f_code"]?.ToString();
                string f_parentcode = row["f_parentcode"]?.ToString();
                List<string> tmp = new List<string>();
                tmp.Add(f_code);
                GetParentIDs(row, tmp);
                if (codeList.Count == 0 || codeList.Exists(e => tmp.IndexOf(e) >= 0))
                {
                    string parent = GetParentId(f_parentcode);
                    codeList.Add(f_code);
                    codeList.Add(f_parentcode);
                    codeList.Add(parent);
                    address = address.Replace(f_name, "");
                }
            }

            var result = codeList.Where(e => !string.IsNullOrEmpty(e) && e != "1").Distinct().OrderBy(x => x).ToList<string>();
            return result;
        }

        private string GetParentId(string cityCode)
        {
            var row = table.Select($"f_code='{cityCode}'");
            if (row == null || row.Length == 0)
                return "";

            return row[0]["f_parentcode"].ToString();
        }

        public void GetParentIDs(DataRow row, List<string> result)
        {
            if (row == null)
                return;

            string f_parentcode = row["f_parentcode"].ToString();
            //父节点为省
            if (f_parentcode == "1")
                return;

            result.Add(f_parentcode);
            //匹配子节点
            var rowArr = table.Select($"f_code='{f_parentcode}'");
            if (rowArr == null || rowArr.Length == 0)
                return;

            foreach (DataRow rowItem in rowArr)
            {
                GetParentIDs(rowItem, result);
            }
        }

        public string GetCityName(List<string> codeList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var code in codeList)
            {
                var row = table.Select($"f_code='{code}'");
                if (row != null && row.Length > 0)
                {
                    if (sb.Length == 0)
                        sb.Append(row[0]["f_name"].ToString() + "省");
                    else
                        sb.Append("-" + row[0]["f_name"].ToString());
                }
            }
            return sb.ToString().Trim();
        }

        private bool MatchName(string address, string name, int type)
        {
            if (type == 2)//省
            {
                int index = address.IndexOf(name);
                //省没有匹配到首字符
                if (index < 0 || index >= 3)
                    return false;
            }
            else if (type == 3)//市
            {
                address = address.Replace("市", "");
                int index = address.IndexOf(name);
                if (index >= 0)
                    return true;
            }
            else if (type == 4)
            {

            }
            return address.IndexOf(name) >= 0;

        }

    }

    public class tbmain_area
    {

        /// <summary>
        /// 区域类型.area区域 1:country/国家;2:province/省/自治区/直辖市;3:city/地区(省下面的地级市);4:district/县/市(县级市)/区;abroad:海外.比如北京市的area_type = 2,朝阳区是北京市的一个区,所以朝阳区的area_type = 4.
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 父节点区域标识
        /// </summary>
        public string parentcode { get; set; }
        /// <summary>
        /// 标准行政区域代码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 地域名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string zip { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        public List<tbmain_area> children { get; set; }

    }
}
