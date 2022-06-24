using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Project.Demo.Client
{
    public class Demo
    {
        static int Id;
        public Demo(int id)
        {
            Id = id;
        }

        public static void TestDemo()
        {
            var date = DateTime.Now.AddDays(1 - DateTime.Now.Day).AddMonths(1).AddDays(-1);
            List<string> moduleList = new List<string>();
            moduleList.Add("aaaaa");
            moduleList.Add("bbbbb");
            string ids = "";
            foreach (var item in moduleList)
            {
                ids += $"'{item}',";
            }
            string _tempsql = " update tb___sys_fin_closefin set f_closefindate=@f_closefindate where f_modulecode in ('"+ string.Join(",", moduleList)+"') ";
        }
    }

    public enum TpyeNum
    {
        one = 1,
        two = 2,
        three = 3,
    }
}
