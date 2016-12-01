using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace log4net_loggly_console
{
    class TestObject
    {
        public string TField1 { get; set; }
        public string TField2 { get; set; }
        public string TField3 { get; set; }
        public object TOField4 { get; set; }

        public TestObject()
        {
            TField1 = "testValue1";
            TField2 = "testValue2";
            TField3 = string.Empty;
            TOField4 = new { TOFF1 = "TOFFValue1", TOFF2 = "TOFFValue2" };
        }
    }
	//test self referencing
	class Person
	{
		public string Name;
		public Person Parent;
		public List<Person> Children;
	}

	class Child : Person
	{
		public string Name;

	}
}
