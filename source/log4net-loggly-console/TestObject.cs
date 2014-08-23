using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace log4net_loggly_console
{
    class TestObject
    {
        public string TestField1 { get; set; }
        public string TestField2 { get; set; }
        public string TestField3 { get; set; }
        public object TestObjectField4 { get; set; }

        public TestObject()
        {
            TestField1 = "testValue1";
            TestField2 = "testValue2";
            TestField3 = string.Empty;
            TestObjectField4 = null;
        }
    }
}
