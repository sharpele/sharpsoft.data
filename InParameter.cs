using System;
using System.Collections;
using System.Text;

namespace SharpSoft.Data
{
    [Obsolete("暂时无法实现将参数列表直接赋值给DbParameter")]
    public class InParameter
    {
        public InParameter()
        {

        }
        public IEnumerable Paras { get; set; }

    }
}
