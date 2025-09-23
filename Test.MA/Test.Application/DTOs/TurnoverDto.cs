namespace Test.Application.DTOs
{
    public class TurnoverDto
    {
        public int Id { get; set; }
        public int? Group_ID { get; set; }
        public int? Class_ID { get; set; }
        public int Account { get; set; }
        public double Start_Active { get; set; }
        public double Start_Passive { get; set; }
        public double Turn_Debit { get; set; }
        public double Turn_Credit { get; set; }
    }
}
