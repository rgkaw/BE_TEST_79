namespace TEST_BE_79_RAKA.Model
{
    public class Money 
    {
        private Decimal nominal { get; set; }
        public Money(Decimal d) 
        { 
            if (d.Equals(null))
            {
                nominal = 0; 
            } 
            else 
            { 
                nominal = d; 
            } 
        }

    }
}
