using Test.UI.Data;

namespace Test.UI.Models
{
    // класс для вывода данных на web 
    public class MyViewModel
    {
        //функция для вывода данных
        public List<Constructor> Print( List<Turnover> data, DBContext _context)
        {
            string help = "";
            int count_help = 0;
            int count_help2 = 0;
            int count = data.Count;
            string name_help = "";
            List<Constructor> it = new();
            Constructor helper = new();
            foreach (var item in data.GroupBy(c=>c.Account/1000))
                foreach (var c in item)
                {
                    // добавление новой строчки со счетом
                    Constructor constructor = new()
                    {
                        CL = _context.Oper_Classes.FirstOrDefault(t => t.Id == (c.Account/1000)).Name,
                        B_sch = c.Account.ToString(),
                        VS_A =c.Start_Active,
                        VS_P = c.Start_Passive,
                        O_D = c.Turn_Debit,
                        O_C = c.Turn_Credit,
                        IS_A = helper.set_IS_A(c.Start_Active, c.Turn_Credit, c.Turn_Debit),
                        IS_P = helper.set_IS_P(c.Start_Passive, c.Turn_Credit, c.Turn_Debit)
                    };
                    it.Add(constructor);

                    if (help == $"{it.Last().B_sch[0]}" + $"{it.Last().B_sch[1]}" || count_help == 0)
                    {
                        help = $"{it.Last().B_sch[0]}" + $"{it.Last().B_sch[1]}";
                        count_help++;
                    }
                    else
                    {
                        // добавление новой строчки с группой счетов
                        Constructor constructor1 = new()
                        {
                            CL = it[it.Count -2].CL,
                            B_sch = help,
                            VS_A = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.VS_A),
                            VS_P = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.VS_P),
                            O_D = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.O_D),
                            O_C = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.O_C),
                            IS_A = helper.set_IS_A(it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.VS_A),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_C),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_D)),
                            IS_P = helper.set_IS_P(it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.VS_P),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_C),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_D))
                        };
                        count_help++;
                        help = $"{it.Last().B_sch[0]}" + $"{it.Last().B_sch[1]}";
                        //it.Add(constructor1);
                        it.Insert(it.Count - 1, constructor1);


                        if (name_help == it.Last().CL || count_help2 == 0)
                        {
                            name_help = it.Last().CL;
                            count_help2++;
                        }
                        else
                        {
                            Constructor constructor2 = new()
                            {
                                // добавление новой строчки с группировкой по классу
                                CL = name_help,
                                B_sch = "ПО КЛАССУ",
                                VS_A = it.Where(d => d.CL == name_help)
                                .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                                .Sum(c => c.VS_A),
                                VS_P = it.Where(d => d.CL == name_help)
                                .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                                .Sum(c => c.VS_P),
                                O_D = it.Where(d => d.CL == name_help)
                                .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                                .Sum(c => c.O_D),
                                O_C = it.Where(d => d.CL == name_help)
                                .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                                .Sum(c => c.O_C),
                                IS_A = helper.set_IS_A(
                                    it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.VS_A),
                                    it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_C),
                                    it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_D)
                                ),
                                IS_P = helper.set_IS_P(
                                    it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.VS_P),
                                    it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_C),
                                    it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_D)
                                    )
                            };
                            name_help = it.Last().CL;
                            count_help2++;
                            it.Add(constructor2);
                            //it.Insert(it.Count - 1, constructor2);
                        }
                    }
                    if (count_help == count)
                    {
                        Constructor constructor3 = new()
                        {
                            // добавление новой строчки с группой счетов
                            CL = name_help,
                            B_sch = help,
                            VS_A = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.VS_A),
                            VS_P =it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.VS_P),
                            O_D = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.O_D),
                            O_C = it.Where(d => d.B_sch.Substring(0, 2) == help)
                            .Sum(d => d.O_C),
                            IS_A = helper.set_IS_A(it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.VS_A),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_C),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_D)),
                            IS_P = helper.set_IS_P(it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.VS_P),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_C),
                            it.Where(d => d.B_sch.Substring(0, 2) == help).Sum(on => on.O_D))
                        };
                        it.Add(constructor3);
                        // добавление новой строчки с группировкой по классу
                        Constructor constructor4 = new()
                        {
                            CL = name_help,
                            B_sch = "ПО КЛАССУ",
                            VS_A = it.Where(d => d.CL == name_help)
                            .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                            .Sum(c => c.VS_A),
                            VS_P = it.Where(d => d.CL == name_help)
                            .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                            .Sum(c => c.VS_P),
                            O_D = it.Where(d => d.CL == name_help)
                            .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                            .Sum(c => c.O_D),
                            O_C = it.Where(d => d.CL == name_help)
                            .Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100)
                            .Sum(c => c.O_C),
                            IS_A = helper.set_IS_A(
                                it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.VS_A),
                                it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_C),
                                it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_D)
                            ),
                            IS_P = helper.set_IS_P(
                                it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.VS_P),
                                it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_C),
                                it.Where(d => d.CL == name_help).Where(c => int.TryParse(c.B_sch, out int b_sch) && b_sch > 100).Sum(c => c.O_D)
                                )
                        };
                        it.Add(constructor4);
                        // добавление новой строчки с итоговыми числами
                        Constructor constructor89 = new()
                        {
                            CL = name_help,
                            B_sch = "БАЛАНС",
                            VS_A = it.Where(d => d.B_sch == "ПО КЛАССУ")
                            .Sum(c => c.VS_A),
                            VS_P = it.Where(d => d.B_sch == "ПО КЛАССУ")
                            .Sum(c => c.VS_P),
                            O_D = it.Where(d => d.B_sch == "ПО КЛАССУ")
                            .Sum(c => c.O_D),
                            O_C = it.Where(d => d.B_sch == "ПО КЛАССУ")
                            .Sum(c => c.O_C),
                            IS_A = helper.set_IS_A(
                                it.Where(d => d.B_sch == "ПО КЛАССУ").Sum(c => c.VS_A),
                                it.Where(d => d.B_sch == "ПО КЛАССУ").Sum(c => c.O_C),
                                it.Where(d => d.B_sch == "ПО КЛАССУ").Sum(c => c.O_D)
                            ),
                            IS_P =helper.set_IS_P(
                                it.Where(d => d.B_sch == "ПО КЛАССУ").Sum(c => c.VS_P),
                                it.Where(d => d.B_sch == "ПО КЛАССУ").Sum(c => c.O_C),
                                it.Where(d => d.B_sch == "ПО КЛАССУ").Sum(c => c.O_D)
                                )
                        };
                        it.Add(constructor89);
                        return it;
                    }
                }
            return it;
        }
    }
    // класс хранения полных данных для вывода
    public class Constructor
    {
        public string CL { get; set; } //класс
        public string B_sch { get; set; } // номер счёта
        //public int B_sch_gr { get; set; }
        public double VS_A { get; set; } //входящее сальдо актив
        public double VS_P { get; set; } // пассив
        public double O_D { get; set; } //оборот дебет
        public double O_C { get; set; } //оборот кредит
        public double IS_A { get; set; }
        public double IS_P { get; set; }
        // функции расчёта исходящего сальдо
        //Актив
        public double set_IS_A(double VS_A, double O_C, double O_D)
        {
            if (VS_A == 0)
            {
                IS_A = 0;
            }
            else
            {
                IS_A = O_D - O_C + VS_A;
            }
            return IS_A;
        }
        //Пассив
        public double set_IS_P(double VS_P, double O_C, double O_D)
        {
            if (VS_P == 0)
            {
                IS_P = 0;
            }
            else
            {
                IS_P = -O_D + O_C + VS_P;
            }
            return IS_P;
        }
    }
}
