using Test.Core.Models;

namespace Test.Application.ViewModels
{
    public class MyViewModel
    {
        public List<Constructor> Print(List<Turnover> data)
        {
            if (data == null || !data.Any())
                return new List<Constructor>();

            var result = new List<Constructor>();
            string currentClass = string.Empty;
            string currentGroup = string.Empty;
            var classData = new List<Constructor>();
            var groupData = new List<Constructor>();

            // Группируем данные по классам (первая цифра счета)
            var classGroups = data.GroupBy(t => t.Account / 1000)
                                 .OrderBy(g => g.Key);

            foreach (var classGroup in classGroups)
            {
                currentClass = classGroup.Key.ToString();
                classData.Clear();

                // Группируем внутри класса по группам (первые две цифры счета)
                var groupGroups = classGroup.GroupBy(t => t.Account / 100)
                                           .OrderBy(g => g.Key);

                foreach (var groupGroup in groupGroups)
                {
                    currentGroup = groupGroup.Key.ToString("00");
                    groupData.Clear();

                    // Добавляем индивидуальные счета
                    foreach (var turnover in groupGroup.OrderBy(t => t.Account))
                    {
                        var constructor = CreateConstructorFromTurnover(turnover, currentClass);
                        result.Add(constructor);
                        groupData.Add(constructor);
                        classData.Add(constructor);
                    }

                    // Добавляем итоги по группе
                    var groupSummary = CreateGroupSummary(groupData, currentClass, currentGroup);
                    result.Add(groupSummary);
                }

                // Добавляем итоги по классу
                var classSummary = CreateClassSummary(classData, currentClass);
                result.Add(classSummary);
            }

            // Добавляем общий баланс
            var balanceSummary = CreateBalanceSummary(result);
            result.Add(balanceSummary);

            return result;
        }

        private Constructor CreateConstructorFromTurnover(Turnover turnover, string classId)
        {
            var constructor = new Constructor
            {
                CL = classId,
                B_sch = turnover.Account.ToString(),
                VS_A = turnover.Start_Active,
                VS_P = turnover.Start_Passive,
                O_D = turnover.Turn_Debit,
                O_C = turnover.Turn_Credit
            };

            constructor.IS_A = constructor.Set_IS_A(constructor.VS_A, constructor.O_C, constructor.O_D);
            constructor.IS_P = constructor.Set_IS_P(constructor.VS_P, constructor.O_C, constructor.O_D);

            return constructor;
        }

        private Constructor CreateGroupSummary(List<Constructor> groupData, string classId, string groupId)
        {
            var summary = new Constructor
            {
                CL = classId,
                B_sch = $"Группа {groupId}",
                VS_A = groupData.Sum(g => g.VS_A),
                VS_P = groupData.Sum(g => g.VS_P),
                O_D = groupData.Sum(g => g.O_D),
                O_C = groupData.Sum(g => g.O_C)
            };

            summary.IS_A = summary.Set_IS_A(summary.VS_A, summary.O_C, summary.O_D);
            summary.IS_P = summary.Set_IS_P(summary.VS_P, summary.O_C, summary.O_D);

            return summary;
        }

        private Constructor CreateClassSummary(List<Constructor> classData, string classId)
        {
            // Берем только индивидуальные счета (исключаем групповые итоги)
            var individualAccounts = classData.Where(c => c.B_sch.Length > 2).ToList();

            var summary = new Constructor
            {
                CL = classId,
                B_sch = "ПО КЛАССУ",
                VS_A = individualAccounts.Sum(c => c.VS_A),
                VS_P = individualAccounts.Sum(c => c.VS_P),
                O_D = individualAccounts.Sum(c => c.O_D),
                O_C = individualAccounts.Sum(c => c.O_C)
            };

            summary.IS_A = summary.Set_IS_A(summary.VS_A, summary.O_C, summary.O_D);
            summary.IS_P = summary.Set_IS_P(summary.VS_P, summary.O_C, summary.O_D);

            return summary;
        }

        private Constructor CreateBalanceSummary(List<Constructor> allData)
        {
            // Берем только итоги по классам
            var classSummaries = allData.Where(c => c.B_sch == "ПО КЛАССУ").ToList();

            var balance = new Constructor
            {
                CL = "ИТОГО",
                B_sch = "БАЛАНС",
                VS_A = classSummaries.Sum(c => c.VS_A),
                VS_P = classSummaries.Sum(c => c.VS_P),
                O_D = classSummaries.Sum(c => c.O_D),
                O_C = classSummaries.Sum(c => c.O_C)
            };

            balance.IS_A = balance.Set_IS_A(balance.VS_A, balance.O_C, balance.O_D);
            balance.IS_P = balance.Set_IS_P(balance.VS_P, balance.O_C, balance.O_D);

            return balance;
        }
    }
}