namespace Test.Core.Models
{
    public class Constructor
    {
        public string CL { get; set; } = string.Empty;
        public string B_sch { get; set; } = string.Empty;
        public double VS_A { get; set; }
        public double VS_P { get; set; }
        public double O_D { get; set; }
        public double O_C { get; set; }
        public double IS_A { get; set; }
        public double IS_P { get; set; }

        public double Set_IS_A(double vsA, double oC, double oD)
        {
            return vsA == 0 ? 0 : oD - oC + vsA;
        }

        public double Set_IS_P(double vsP, double oC, double oD)
        {
            return vsP == 0 ? 0 : -oD + oC + vsP;
        }
    }
}
