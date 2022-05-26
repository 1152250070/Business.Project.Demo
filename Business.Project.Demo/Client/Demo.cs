using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Project.Demo.Client
{
    public class Demo
    {
        public static void TestDemo()
        {
            string str = "2022-05-26 13:54:19";
            var date = Convert.ToDateTime(str).ToString("yyyy-MM-dd HH:mm:ss");
            TpyeNum num = TpyeNum.one | TpyeNum.one | TpyeNum.three;
            if (num== TpyeNum.one)
            {

            }
        }
    }

    public enum TpyeNum
    {
        one = 1,
        two = 2,
        three = 3,
    }
}
