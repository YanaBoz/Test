using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Application.Interfaces;
using Test.Core.Models;

namespace Test.Application.Services
{
    public class TurnoverService : ITurnoverService
    {
        // Обработка данных оборотов и построение view-моделей
        public async Task<List<Constructor>> ProcessTurnoverDataAsync(IEnumerable<Turnover> data)
        {
            return await Task.Run(() => BuildViewData(data.ToList()));
        }

        // Построение структуры данных для отображения
        private List<Constructor> BuildViewData(List<Turnover> data)
        {
            var result = new List<Constructor>();
            var sectionSums = new Dictionary<int, Constructor>();

            // Группировка по "сотням" счета (отбрасываем последние 2 цифры)
            var groups = data.GroupBy(c => c.Account / 100).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                int section = int.Parse(group.Key.ToString().Substring(0, 1)); // Определяем раздел
                var tempList = new List<Constructor>();

                foreach (var c in group)
                {
                    double is_a = c.Start_Active == 0 ? 0 : c.Start_Active + c.Turn_Debit - c.Turn_Credit;
                    double is_p = c.Start_Passive == 0 ? 0 : c.Start_Passive + c.Turn_Credit - c.Turn_Debit;

                    tempList.Add(new Constructor
                    {
                        CL = section.ToString(),
                        B_sch = c.Account.ToString(),
                        VS_A = c.Start_Active,
                        VS_P = c.Start_Passive,
                        O_D = c.Turn_Debit,
                        O_C = c.Turn_Credit,
                        IS_A = is_a,
                        IS_P = is_p
                    });
                }

                result.AddRange(tempList);

                // Суммирование по группе
                var groupSummary = new Constructor
                {
                    CL = section.ToString(),
                    B_sch = group.Key.ToString(),
                    VS_A = tempList.Sum(x => x.VS_A),
                    VS_P = tempList.Sum(x => x.VS_P),
                    O_D = tempList.Sum(x => x.O_D),
                    O_C = tempList.Sum(x => x.O_C),
                    IS_A = tempList.Sum(x => x.IS_A),
                    IS_P = tempList.Sum(x => x.IS_P)
                };
                result.Add(groupSummary);

                // Аккумулирование сумм для раздела
                if (!sectionSums.ContainsKey(section))
                {
                    sectionSums[section] = new Constructor
                    {
                        CL = section.ToString(),
                        B_sch = section.ToString(),
                        VS_A = 0,
                        VS_P = 0,
                        O_D = 0,
                        O_C = 0,
                        IS_A = 0,
                        IS_P = 0
                    };
                }

                var sec = sectionSums[section];
                sec.VS_A += groupSummary.VS_A;
                sec.VS_P += groupSummary.VS_P;
                sec.O_D += groupSummary.O_D;
                sec.O_C += groupSummary.O_C;
                sec.IS_A += groupSummary.IS_A;
                sec.IS_P += groupSummary.IS_P;
            }

            // Добавляем итоговые суммы по разделам
            result.AddRange(sectionSums.Values.OrderBy(x => x.CL));

            return result;
        }
    }
}
