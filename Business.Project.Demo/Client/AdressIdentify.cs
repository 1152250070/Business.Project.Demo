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
            string address = "江苏南京浦口区江浦街道";
            string cityf_name = addressModel.GetCityId(address);
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
        //最多返回3级编码，省市区
        private int MAX_CITYS = 3;
        //根节点编码
        private string ROOT_ID = "100000";

        private DataTable table;

        public AddressToCityIDs(DataTable dtCitys)
        {
            this.table = dtCitys;
        }

        /// <summary>
        /// 获取上级区域编号
        /// </summary>
        /// <param f_name="cityf_code"></param>
        /// <returns></returns>
        private string GetParentId(string cityf_code)
        {
            var row = table.Select($"f_code='{cityf_code}'");
            if (row != null && row.Length > 0)
                return row[0]["f_parentcode"].ToString();
            else
                return "";
        }

        /// <summary>
        /// 获取指定区域码的所有父级编码
        /// </summary>
        /// <param f_name="result">返回结果</param>
        /// <param f_name="cityf_code">区域码</param>
        public void GetParentIDs(List<String> result, string cityCode)
        {
            var row = table.Select($"f_code='{cityCode}'");
            if (row != null && row.Length > 0)
            {
                string f_parentcode = row[0]["f_parentcode"].ToString();
                //父节点为省
                if (f_parentcode == "1")
                    return;

                result.Add(f_parentcode);
                //匹配子节点
                table.DefaultView.RowFilter = $"f_code='{f_parentcode}'";
                var dt = table.DefaultView.ToTable();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow R in dt.Rows)
                    {
                        GetParentIDs(result, R["f_code"].ToString());
                    }
                }
            }
            //不包含根节点
            result.Remove(ROOT_ID);
        }

        /// <summary>
        /// 获取省市名称
        /// </summary>
        /// <param f_name="f_codeList">区域编码</param>
        /// <returns></returns>
        public string GetCityf_name(List<string> codeList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var code in codeList)
            {
                var row = table.Select($"f_code='{code}'");
                if (row.Length > 0)
                {
                    if (sb.Length == 0)
                        sb.Append(row[0]["f_name"].ToString());
                    else
                        sb.Append("-" + row[0]["f_name"].ToString());
                }
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// 根据地址信息，自动获取行政区域编码
        /// </summary>        
        /// <param f_name="address">地址信息</param>
        /// <param f_name="cityIDs">输出3个城市编码，调号隔开</param>
        /// <param f_name="cityf_names">输出3个城市名称，调号隔开</param>
        /// <returns></returns>
        public string GetCityId(string address)
        {
            List<String> ids = new List<string>();

            if (address.IndexOf("/") > 0)
                ids = GetCityIDsByAddress(address, '/', ' ');
            else
                ids = GetCityIDsByAddress(address);

            var result = ids.Take<String>(MAX_CITYS).ToList<String>();
            //返回结果
            string cityCodes = String.Join(",", result).Trim();//310000,310100,310116
            string cityNames = GetCityf_name(result);//上海市-市辖区-金山区
            return cityNames;
        }

        /// <summary>
        /// 获取地址区码,地址有分隔符，如：/
        /// </summary>
        /// <param f_name="address">地址，如：上海市/松江区 松卫北路6700弄沪松五金建材市场3幢</param>
        /// <returns>返回区域码列表，如：310000/310100/310117</returns>
        public List<String> GetCityIDsByAddress(string address, params char[] separator)
        {
            void AddIDs(List<String> ids, DataRow row)
            {
                List<string> tmp = new List<string>();
                GetParentIDs(tmp, row["f_code"].ToString());
                tmp.Add(row["f_code"].ToString());

                if (ids.Count == 0 || ids.Exists(e => tmp.IndexOf(e) >= 0))
                {
                    ids.Add(row["f_code"].ToString());
                    ids.Add(row["f_parentcode"].ToString());
                    ids.Add(GetParentId(row["f_parentcode"].ToString()));
                }
            }
            List<String> result = new List<string>();
            //分解地址            
            var list = address.Split(separator, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            list = list.Distinct().ToArray();//去重,唯一名称
            //有规则，有分隔符号/，如：上海市/上海市/松江区 松卫北路6700弄沪松五金建材市场3幢1*****
            if (list.Length > 1)
            {
                DataRow[] row;
                foreach (var item in list)
                {
                    row = table.Select($"f_name='{item.Trim()}'", "f_type ASC");//匹配名称成功，如：上海市、松江区
                    if (row.Length == 1)
                    {
                        AddIDs(result, row[0]);
                    }
                    else if (row.Length > 1)
                    {
                        foreach (DataRow R in row)
                        {
                            List<string> tmp = new List<string>();
                            GetParentIDs(tmp, R["f_code"].ToString());
                            if (result.Exists(e => tmp.IndexOf(e) >= 0))
                            {
                                AddIDs(result, R);
                                break;
                            }
                        }
                    }
                }
            }

            result = result.Where(e => e != "" && e != ROOT_ID).Distinct().OrderBy(x => x).ToList<String>();
            return result;
        }

        /// <summary>
        /// 获取地址区码
        /// </summary>
        /// <param f_name="address">地址，如：上海市松江区松卫北路6700弄沪松五金建材市场3幢</param>
        /// <returns>返回区域码列表，如：310000/310100/310117</returns>
        public List<String> GetCityIDsByAddress(string address)
        {
            List<String> ids = new List<string>();
            table.DefaultView.Sort = "f_type ASC";
            //枚举所有城市名称，匹配地址
            foreach (DataRow R in table.DefaultView.ToTable().Rows)
            {
                var f_name = R["f_name"].ToString();
                //上海市宝山区丰翔路888号
                if (address.IndexOf(f_name) >= 0)
                {
                    string f_code = R["f_code"]?.ToString();
                    string f_parentcode = R["f_parentcode"]?.ToString();
                    List<string> tmp = new List<string>();
                    GetParentIDs(tmp, f_code);
                    tmp.Add(f_code);
                    if (ids.Count == 0 || ids.Exists(e => tmp.IndexOf(e) >= 0))
                    {
                        string parent = GetParentId(f_parentcode);
                        ids.Add(f_code);
                        ids.Add(f_parentcode == "1" ? "" : f_parentcode);
                        ids.Add(parent == "1" ? "" : parent);
                        address = address.Replace(f_name, "");
                    }
                }
            }

            var result = ids.Where(e => e != "" && e != ROOT_ID).Distinct().OrderBy(x => x).ToList<String>();
            return result;
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
