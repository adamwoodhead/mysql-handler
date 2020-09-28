using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQL_Handler;

namespace Testing_Console_App
{
    [SqlTable("people")]
    internal class Person : SqlClass<Person>
    {
        [SqlColumn("id", SqlVarType.INT12, false, FieldOptions.PRIMARY)]
        internal int? ID { get; set; }

        [SqlColumn("name", SqlVarType.VARCHAR120, false)]
        internal string Name { get; set; }

        [SqlColumn("age", SqlVarType.VARCHAR120)]
        internal int? Age { get; set; }

        internal Person()
        {
            Syncing += new EventHandler(PreSync);
            Inserted += new EventHandler(PostInsert);
        }

        internal Person(string name, int? age)
        {
            Name = name;
            Age = age;

            Syncing += new EventHandler(PreSync);
            Inserted += new EventHandler(PostInsert);
        }

        private void PreSync(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("Do some funky stuff pre sync (before insert OR update");
        }

        private void PostInsert(object sender, EventArgs e)
        {
            Console.WriteLine("Do some funky stuff after inserting");
        }

        public override void Delete()
        {
            // Do we need to do anything before dropping this object/record?
            this.Drop();
        }
    }
}
