using DocumentFormat.OpenXml.ExtendedProperties;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.UI.Models
{
    //класс для хранения данных по счетам
    public class Turnover
    {
        public int Id { get; set; }
        public int? Group_ID { get; set; }
        [ForeignKey("Group_ID")]
        public Group Group { get; set; }
        public int? Class_ID { get; set; }
        [ForeignKey("Class_ID")]
        public Oper_Class Oper_Class { get; set; }
        public int Account { get; set; }
        public double Start_Active { get; set; }
        public double Start_Passive { get; set; }
        public double Turn_Debit { get; set; }
        public double Turn_Credit { get; set; }
    }
    // класс для хранения названий классов
    public class Oper_Class
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    // класс для хранения номеров групп
    public class Group
    {
        public int Id { get; set; }
        public int Number { get; set; }
    }
    // класс для хранения файлов
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
