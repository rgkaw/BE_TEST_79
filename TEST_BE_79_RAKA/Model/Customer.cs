using System.ComponentModel.DataAnnotations;

namespace TEST_BE_79_RAKA.Model
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
    }
    //internal class CustomerIdGenerator
    //{
    //    private IdGenerator idGenerator;
    //    public CustomerIdGenerator() { 
    //        this.idGenerator=new IdGenerator();
    //}

    //    public int generateId()
    //    {
    //        return this.idGenerator.getId();
    //    }
    //}
}