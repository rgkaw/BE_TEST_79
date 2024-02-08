using System.CodeDom.Compiler;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace TEST_BE_79_RAKA.Model
{
    public class IdGenerator
    {
        private int Id;
        public IdGenerator() { this.Id = 1; }
        public int getId()
        {
            this.Id++;
            return this.Id-1;
        }
    }
}
