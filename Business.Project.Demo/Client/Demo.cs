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
            Id=id;
        }

        public static void TestDemo()
        {
            var num1 = Id;
            string str = "2022-05-26 13:54:19";
            string str2 = @"test11test11test1test11test11test11test111test11test11test11test1test11test11test11test111test11test11test11test1test11test11";
            int num = str2.Length;
            var date = Convert.ToDateTime(str).ToString("yyyy-MM-dd HH:mm:ss");
            
        }
    }

    public enum TpyeNum
    {
        one = 1,
        two = 2,
        three = 3,
    }
}
