using Business.Project.Demo.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business.Project.Demo.Client
{
    public class AddressMatch
    {
        public static void GetAdress()
        {
            string content = FileUtil.GetContent();
            var areaList = JsonConvert.DeserializeObject<List<tbmain_area>>(content) ?? new List<tbmain_area>();
            AddressMatch addressMatch = new AddressMatch();
            string address = "安徽省黄山市屯溪区昱东街道昱东街道黄山东路55号黄山供电公司 倪媛媛 18855942632";//"江苏南京浦口区江浦街道";//杭州舟山东路
            bool isExciel = address.StartsWith("江西");
            string str1 = address.TrimEnd(new char[] { '村', '庄' });
            string cityf_name = addressMatch.GetCityName(areaList, address);
        }


        private string GetCityName(List<tbmain_area> areaList, string address)
        {
            if (areaList == null || areaList.Count == 0)
            {
                return "";
            }
            //匹配省市区
            string provinceCode = GetProvinceCode(areaList, address);
            string cityCode = GetCityCode(areaList, address, ref provinceCode);
            string areaCode = GetAreaCode(areaList, ref address, ref provinceCode, ref cityCode);
            string provinceName = GetAreaName(areaList, provinceCode);
            string cityName = GetAreaName(areaList, cityCode);
            string areaName = GetAreaName(areaList, areaCode);
            //如果需要匹配街道需要根据市和区编号区重新查库
            if (!string.IsNullOrEmpty(areaCode))
            {

            }
            return provinceName + "-" + cityName + "-" + areaName;
        }

        private string GetProvinceCode(List<tbmain_area> areaList, string address)
        {
            var list = areaList.Where(a => a.type == 2).ToList();
            if (list == null || list.Count == 0)
            {
                return "";
            }
            foreach (tbmain_area area in list)
            {
                int index = address.IndexOf(area.name);
                if (index < 0 || index >= 3)
                    continue;

                return area.code;
            }
            return "";
        }

        private string GetCityCode(List<tbmain_area> areaList, string address, ref string provinceCode)
        {
            var list = areaList.Where(a => a.type == 3).ToList();
            if (!string.IsNullOrEmpty(provinceCode))
            {
                string parentCode = provinceCode;
                list = areaList.Where(a => a.parentcode == parentCode).ToList();
            }
            if (list == null || list.Count == 0)
            {
                return "";
            }
            foreach (tbmain_area area in list)
            {
                string name = area.name.Replace("市", "");
                int index = address.IndexOf(name);
                if (index >= 0)
                {
                    provinceCode = area.parentcode;
                    return area.code;
                }
            }
            return "";
        }

        private string GetAreaCode(List<tbmain_area> areaList, ref string address, ref string provinceCode, ref string cityCode)
        {
            var list = areaList.Where(a => a.type == 4).ToList();
            if (!string.IsNullOrEmpty(cityCode))
            {
                string parentCode = cityCode;
                list = areaList.Where(a => a.parentcode == parentCode).ToList();
            }
            if (list == null || list.Count == 0)
            {
                return "";
            }
            foreach (tbmain_area area in list)
            {
                string name = area.name.Replace("区", "");
                int index = address.IndexOf(name);
                if (index >= 0)
                {
                    cityCode = area.parentcode;
                    if (string.IsNullOrEmpty(provinceCode))
                    {
                        var province = areaList.Where(a => a.code == area.parentcode).FirstOrDefault() ?? new tbmain_area();
                        provinceCode = province.code;
                    }
                    //删除省市区
                    address = address.Remove(0, index);
                    address = address.Replace(area.name, "").Replace(name, "");
                    return area.code;
                }
            }
            return "";
        }

        private string GetAreaName(List<tbmain_area> areaList, string code)
        {
            string name = areaList.Where(a => a.code == code).Select(a => a.name).FirstOrDefault() ?? "";
            return name;
        }
    }

    public class AddressParseV2
    {
        // static IMemoryCache _memeryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        public static ReceiverModel AutoSplitReveive()
        {
            string orifulladdress = "张洒洒水，13588403906，江苏南京浦口区江浦街道文三西路110号";
            ReceiverModel rmodel = new ReceiverModel();
            GetReveiveList(orifulladdress.Trim(), out string phone, out string name, out string addressDetail);
            addressDetail = addressDetail.TrimStart(new char[] { ',', '，', ' ' });
            name = name.Replace(",", "").Replace("，", "");
            if (string.IsNullOrEmpty(addressDetail))
                return rmodel;

            string address = GetCityName(addressDetail, out string provinceName, out string cityName, out string areaName, out string rstreet);
            rmodel.rmobile = phone;
            rmodel.rname = name;
            rmodel.rprovince = provinceName;
            rmodel.rcity = cityName;
            rmodel.rdistrict = areaName;
            rmodel.rstreet = rstreet;
            rmodel.rdetail = address;
            return rmodel;
        }

        public static void GetReveiveList(string address, out string mobile, out string name, out string addressDetail)
        {
            Regex regexMobile = new Regex(@"(1[3|4|5|6|7|8|9]\d{9})|(0\d{2,3}-\d{7,8})|(400-\d{3}-\d{4})|(400\d{7})");
            Regex regexName = new Regex(@"^[A-Za-z0-9]+$");
            var provinceList = new List<string>() { "北京", "天津", "河北", "山西", "内蒙", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东", "河南", "湖北", "湖南", "广东", "广西", "海南", "重庆", "四川", "贵州", "云南", "西藏", "陕西", "甘肃", "青海", "宁夏", "新疆", "台湾", "香港", "澳门" };
            MatchCollection collection = regexMobile.Matches(address);
            name = "";
            addressDetail = "";
            int mobileIndex = collection[0].Index;//21
            int totalLength = address.Length;//35
            mobile = collection == null || collection.Count() == 0 ? "" : collection[0].Value;
            int mobileLength = mobile.Length;//11
            if (!string.IsNullOrEmpty(mobile))
            {
                address = address.Replace("+86" + mobile, "★").Replace("86" + mobile, "★").Replace(mobile, "★");
            }
            var addresList = address.Split("★").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            //姓名+电话+地址 || 地址+电话+姓名     电话在中间
            int nameAndMobileLength = mobileIndex + mobileLength;
            if ((nameAndMobileLength - 5 < totalLength - nameAndMobileLength && addresList.Count == 2) ||
                nameAndMobileLength > totalLength - nameAndMobileLength && addresList.Count == 2)
            {
                for (int i = 0; i < addresList.Count; i++)
                {
                    if (regexName.IsMatch(addresList[i]))//英文
                    {
                        name = addresList[i];
                        addressDetail = i == 0 ? addresList[i + 1] : addresList[i - 1];
                        return;
                    }
                }
                name = addresList[0].Length > addresList[1].Length ? addresList[1] : addresList[0];
                addressDetail = addresList[0].Length < addresList[1].Length ? addresList[1] : addresList[0];
            }
            //电话+姓名+地址 || 地址+姓名+电话 || 姓名+地址+电话 || 电话+地址+姓名
            else if ((mobileIndex == 0 && addresList.Count == 1) ||
                (nameAndMobileLength == totalLength && addresList.Count == 1))
            {
                string nameAddress = addresList[0];
                var addressArr = nameAddress.Split(", ，;；　\r\n\t".ToArray(), StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (addressArr.Count <= 1)
                {
                    AddressSplitName(provinceList, nameAddress, out name, out addressDetail);
                    return;
                }
                //地址+姓名+电话
                foreach (var province in provinceList)
                {
                    if (nameAddress.StartsWith(province))
                    {
                        name = addressArr[addressArr.Count - 1];
                        for (int i = 0; i < addressArr.Count - 1; i++)
                        {
                            addressDetail += addressArr[i];
                        }
                        return;
                    }
                }
                //电话 + 姓名 + 地址
                AddressSplitName(provinceList, nameAddress, out name, out addressDetail);
                return;
            }
        }

        //姓名 + 地址
        private static void AddressSplitName(List<string> provinceList, string address, out string nameSplit, out string addressSplit)
        {
            Regex regexName = new Regex("^[A-Za-z0-9]+$");
            nameSplit = "";
            addressSplit = "";
            if (string.IsNullOrEmpty(address) || provinceList == null || provinceList.Count == 0)
                return;

            foreach (var province in provinceList)
            {
                if (address.StartsWith(province))//地址+姓名 没有空格
                {
                    nameSplit = "";
                    addressSplit = address;
                    break;
                }
                int index = address.IndexOf(province); //姓名+地址 
                //判断是否是英文和数字
                if (index > 5)
                {
                    string numOrZ = address.Substring(5, index - 5);
                    if (regexName.IsMatch(numOrZ))
                    {
                        nameSplit = address.Substring(0, index);
                        addressSplit = address.Remove(0, index);
                        break;
                    }
                }
                if (index >= 2 && index < 11)
                {
                    nameSplit = address.Substring(0, index);
                    addressSplit = address.Remove(0, index);
                    break;
                }
            }
        }

        private static void GetMobileCode(List<string> items, ReceiverModel rmodel)
        {
            var regmobile = new System.Text.RegularExpressions.Regex("^((\\+)?86)?1[0-9]{10}$");
            var regmobile1 = new System.Text.RegularExpressions.Regex("((\\+)?86)?1[0-9]{10}");
            var regphone = new System.Text.RegularExpressions.Regex("^(0[0-9]{2,3}(-)?)?([2-9][0-9]{6,7})+(/-[0-9]{1,4})?$");
            var regpost = new System.Text.RegularExpressions.Regex("^[0-9]{6}$");
            //手机/电话
            for (var k = 0; k < items.Count; k++)
            {
                var item = items[k];
                if (item == "86" || item == "+86")
                {
                    items.RemoveAt(k);
                    k--;
                    continue;
                }
                if (regmobile.IsMatch(item))
                {
                    rmodel.rmobile = item;
                    items.RemoveAt(k);
                    k--;
                    continue;
                }
                if (regphone.IsMatch(item))
                {
                    rmodel.rphone = item;
                    items.RemoveAt(k);
                    k--;
                    continue;
                }
                if (regpost.IsMatch(item))
                {
                    rmodel.rzip = item;
                    items.RemoveAt(k);
                    k--;
                    continue;
                }
            }
            if (string.IsNullOrWhiteSpace(rmodel.rmobile))
            {
                for (var k = 0; k < items.Count; k++)
                {
                    var item = items[k];

                    if (regmobile1.IsMatch(item))
                    {
                        rmodel.rmobile = regmobile1.Match(item).Value;
                        items.RemoveAt(k);
                        var release = item.Replace(rmodel.rmobile, " ").Trim();
                        var xxitems = release.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (!string.IsNullOrEmpty(release) && xxitems.Length > 0)
                        {
                            items.InsertRange(k, xxitems);
                        }
                        break;
                    }
                }
            }

        }

        private static void GetRName(List<string> items, ReceiverModel rmodel)
        {
            var provinces = new List<string>() { "北京",
"天津","河北","山西","内蒙","辽宁","吉林","黑龙江","上海","江苏","浙江","安徽","福建","江西","山东","河南",
"湖北","湖南","广东","广西","海南","重庆","四川","贵州","云南","西藏","陕西","甘肃","青海","宁夏","新疆","台湾",
"香港","澳门"};
            int provindex = -1;
            int provlength = -1;
            if (items.Count == 0)
                return;

            for (var k = 0; k < items.Count; k++)
            {
                var item = items[k];
                var exist = provinces.FindIndex(x => item.StartsWith(x));
                if (exist >= 0)
                {
                    provlength = provinces[exist].Length;
                    provindex = k;
                    break;
                }
            }
            if (provindex < 0)
            {
                var minLengthItem = items.OrderBy(t => t.Length).First();
                rmodel.rname = minLengthItem;
                rmodel.rdetail = String.Join(' ', items.Where(t => t != minLengthItem));
                return;
            }
            var tmpdetail = string.Empty;
            if (provindex > 0)
            {
                if (IsYinWenMing(items[0]))
                { // 如果是英文名 不限长度
                    rmodel.rname = items[0];
                    items.RemoveAt(0);
                    provindex--;
                }
                else// if (items[0].Length <= 4)
                { // 如果是汉子 小于4个字符 就是名字
                    rmodel.rname = items[0];
                    items.RemoveAt(0);
                    provindex--;
                }
            }
            else if (items.Count > provindex + 1)
            {
                var minLengthItem = items.Skip(provindex).OrderBy(t => t.Length).First();
                rmodel.rname = minLengthItem;
                tmpdetail = String.Join(' ', items.Skip(provindex + 1).Where(t => t != minLengthItem));
            }

        }

        private static string GetCityName(string address, out string provinceName, out string cityName, out string areaName, out string streetName)
        {
            List<tbmain_area> areaList = GetCityList() ?? new List<tbmain_area>();
            //匹配省市区
            string provinceCode = GetProvinceCode(areaList, ref address);
            string cityCode = GetCityCode(areaList, ref address, ref provinceCode);
            string areaCode = GetAreaCode(areaList, ref address, ref provinceCode, ref cityCode);
            string streetCode = GetStreetCode(areaCode, ref address, out streetName);
            provinceName = GetAreaName(areaList, provinceCode);
            cityName = GetAreaName(areaList, cityCode);
            areaName = GetAreaName(areaList, areaCode);

            //如果需要匹配街道需要根据市和区编号区重新查库
            return address;
        }

        private static string GetProvinceCode(List<tbmain_area> areaList, ref string address)
        {
            var list = areaList.Where(a => a.type == 2).ToList();
            var autoRegions = new List<string>() { "新疆维吾尔自治区", "新疆维吾尔族自治区", "广西壮族自治区", "宁夏回族自治区", "西藏自治区", "内蒙古自治区" };
            if (list == null || list.Count == 0)
            {
                return "";
            }
            foreach (tbmain_area area in list)
            {
                string regionName = "";
                string name = area.name.Replace("市", "");
                foreach (var region in autoRegions)
                {
                    if (region.StartsWith(area.name))
                    {
                        regionName = region;
                        break;
                    }
                }
                int removeLength = 0;
                if (!string.IsNullOrEmpty(regionName) && address.StartsWith(regionName))
                {
                    removeLength = regionName.Length;
                }
                else if (address.StartsWith(name))
                {
                    removeLength = name.Length;
                }
                if (removeLength == 0)
                    continue;

                address = address.Remove(0, removeLength);
                if (address.StartsWith("市"))
                {
                    address = address.Replace("市", "");
                }
                else if (address.StartsWith("省"))
                {
                    address = address.Replace("省", "");
                }
                return area.code;
            }
            return "";
        }

        private static string GetCityCode(List<tbmain_area> areaList, ref string address, ref string provinceCode)
        {
            var list = areaList.Where(a => a.type == 3).ToList();
            if (!string.IsNullOrEmpty(provinceCode))
            {
                string parentCode = provinceCode;
                list = areaList.Where(a => a.parentcode == parentCode).ToList();
            }
            if (list == null || list.Count == 0)
            {
                return "";
            }
            foreach (tbmain_area area in list)
            {
                string name = RemoveCityNameEnd(area.name, out string endName);
                int index = address.IndexOf(name);
                if (address.StartsWith(name))
                {
                    address = address.Remove(0, name.Length);
                    address = address.StartsWith(endName) ? address.Remove(0, 1) : address;
                    provinceCode = string.IsNullOrEmpty(provinceCode) ? area.parentcode : provinceCode;
                    return area.code;
                }
            }
            return "";
        }

        private static string GetAreaCode(List<tbmain_area> areaList, ref string address, ref string provinceCode, ref string cityCode)
        {
            var list = areaList.Where(a => a.type == 4).ToList();
            if (string.IsNullOrEmpty(cityCode))
            {
                return "";
            }
            string parentCode = cityCode;
            list = areaList.Where(a => a.parentcode == parentCode).ToList();
            if (list == null || list.Count == 0)
            {
                return "";
            }
            foreach (tbmain_area area in list)
            {
                string name = RemoveAreaNameEnd(area.name, out string endName);
                if (address.StartsWith(name))
                {
                    cityCode = area.parentcode;
                    if (string.IsNullOrEmpty(provinceCode))
                    {
                        var province = areaList.Where(a => a.code == area.parentcode).FirstOrDefault() ?? new tbmain_area();
                        provinceCode = province.code;
                    }
                    address = address.Remove(0, name.Length);
                    address = address.StartsWith(endName) ? address.Remove(0, 1) : address;
                    return area.code;
                }
            }
            return "";
        }

        private static string GetStreetCode(string areaCode, ref string address, out string streetName)
        {
            streetName = "";
            if (string.IsNullOrEmpty(areaCode))
                return "";

            var streetList = GetCityStreetList(areaCode);
            if (streetList == null || streetList.Count == 0)
                return "";

            foreach (var street in streetList)
            {
                if (address.StartsWith(street.name))
                {
                    address = address.Remove(0, street.name.Length);
                    streetName = street.name;
                    return street.code;
                }
            }
            return address;
        }

        private static string GetAreaName(List<tbmain_area> areaList, string code)
        {
            string name = areaList.Where(a => a.code == code).Select(a => a.name).FirstOrDefault() ?? "";
            return name;
        }

        #region 字符串处理
        private static string RemoveCityNameEnd(string address, out string endName)
        {
            endName = "";
            if (string.IsNullOrEmpty(address) || address.Length < 3)
            {
                return address;
            }
            else if (address.EndsWith("市"))
            {
                endName = "市";
                return address.TrimEnd('市');
            }
            else if (address.EndsWith("州"))
            {
                endName = "州";
                return address.TrimEnd('州');
            }
            else if (address.EndsWith("区"))
            {
                endName = "区";
                return address.TrimEnd('区');
            }
            else if (address.EndsWith("县"))
            {
                endName = "县";
                return address.TrimEnd('县');
            }
            return address;
        }

        private static string RemoveAreaNameEnd(string address, out string endName)
        {
            endName = "";
            if (string.IsNullOrEmpty(address) || address.Length < 3)
            {
                return address;
            }
            else if (address.EndsWith("区"))
            {
                endName = "区";
                return address.TrimEnd('区');
            }
            else if (address.EndsWith("县"))
            {
                endName = "县";
                return address.TrimEnd('县');
            }
            else if (address.EndsWith("市"))
            {
                endName = "市";
                return address.TrimEnd('市');
            }
            return address;
        }

        private static bool IsYinWenMing(string mc)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^[\u0000-\u0080]+$");
            return r.IsMatch(mc);
        }
        #endregion

        public static void WriteAddress() 
        {
            List<tbmain_area> areaList = GetCityList() ?? new List<tbmain_area>();
            var privodeList = areaList.Where(a => a.type == 2).ToList();
            var streetList = GetCityStreetList();
            StringBuilder buider = new StringBuilder();
            foreach (var item in privodeList)
            {
                string addressname1 = item.name;
                foreach (var itemcity in areaList.Where(a => a.type == 3).Where(a => a.parentcode == item.code)) 
                {
                    string addressname2 = addressname1+"--"+ itemcity.name;
                    foreach (var itemarea in areaList.Where(a => a.type == 4).Where(a => a.parentcode == itemcity.code))
                    {
                        string addressname3 = addressname2 + "--" + itemarea.name;
                        foreach (var itemstrees in streetList.Where(a => a.parentcode == itemarea.code))
                        {
                            string addressname4 = addressname3 + "--" + itemstrees.name+ "\r\n \r\n";
                            buider.Append(addressname4);
                        }
                    }
                }
            }
            FileUtil.WriteLog(buider.ToString());
        }

        /// <summary>
        /// 查询省市区
        /// </summary>
        /// <returns></returns>
        public static List<tbmain_area> GetCityList()
        {
            string content = FileUtil.GetContent();
            return JsonConvert.DeserializeObject<List<tbmain_area>>(content) ?? new List<tbmain_area>();
        }

        //获取区下面的街道
        public static List<tbmain_area> GetCityStreetList(string parentcode="")
        {
            string content = FileUtil.GetContent(@"D:\Heng_Sheng_DianZi\etc\20220615.txt");
            return JsonConvert.DeserializeObject<List<tbmain_area>>(content) ?? new List<tbmain_area>();
        }

    }

    public class ReceiverModel
    {
        /// <summary>姓名</summary>
        public string rname { get; set; }
        /// <summary>手机</summary>
        public string rmobile { get; set; }
        /// <summary>电话</summary>
        public string rphone { get; set; }
        /// <summary>国家</summary>
        public string rcountry { get; set; }
        /// <summary>省份</summary>
        public string rprovince { get; set; }
        /// <summary>城市</summary>
        public string rcity { get; set; }
        /// <summary>地区</summary>
        public string rdistrict { get; set; }
        /// <summary>街道</summary>
        public string rstreet { get; set; }
        /// <summary>详细地址</summary>
        public string rdetail { get; set; }
        /// <summary>邮编</summary>
        public string rzip { get; set; }

        /// <summary>完整地址，取电子面单时使用</summary>
        public string rfulladdress
        {
            get
            {
                List<string> items = new List<string>();
                items.Add(this.rcountry);
                items.Add(this.rprovince);
                items.Add(this.rcity);
                items.Add(this.rdistrict);
                items.Add(this.rstreet);
                var mdetail = (this.rdetail + "").Trim();
                //淘宝多出来一个街道，要去掉 2019.7.9 14:58 小凤+明娥需求 
                if (!string.IsNullOrWhiteSpace(this.rstreet) && this.rstreet.Length > 2 && mdetail.StartsWith(this.rstreet))
                {
                    mdetail = mdetail.Substring(this.rstreet.Length);
                }
                if (!string.IsNullOrWhiteSpace(mdetail))
                {
                    if (mdetail.StartsWith(string.Join("", items)))
                        items.Clear();
                    items.Add(mdetail);
                }
                items = items.Select(x => (x + "").Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
                return string.Join(" ", items);
            }
        }

        public string ToNameFullAddress()
        {
            return string.Format("{0} {1} {2} {3} {4}", this.rname, this.rmobile, this.rphone, this.rfulladdress, this.rzip);
        }
    }
}
