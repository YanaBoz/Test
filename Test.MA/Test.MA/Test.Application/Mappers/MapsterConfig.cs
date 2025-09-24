using Mapster;
using Test.Application.DTOs;
using Test.Core.Models;

namespace Test.Application.Mappers
{
    public static class MapsterConfig
    {
        public static void Configure(TypeAdapterConfig config)
        {
            // Конфигурация маппинга между Turnover и TurnoverDto
            // PreserveReference(true) сохраняет ссылки на объекты, чтобы избежать дублирования при циклических ссылках
            config.NewConfig<Turnover, TurnoverDto>().PreserveReference(true);
            config.NewConfig<TurnoverDto, Turnover>().PreserveReference(true);

            // Конфигурация маппинга между Constructor и ConstructorDto
            config.NewConfig<Constructor, ConstructorDto>().PreserveReference(true);
            config.NewConfig<ConstructorDto, Constructor>().PreserveReference(true);

            // Конфигурация маппинга между FileModel и FileDto
            config.NewConfig<FileModel, FileDto>().PreserveReference(true);
            config.NewConfig<FileDto, FileModel>().PreserveReference(true);

            // Конфигурация маппинга между Group и GroupDto
            config.NewConfig<Group, GroupDto>().PreserveReference(true);
            config.NewConfig<GroupDto, Group>().PreserveReference(true);

            // Конфигурация маппинга между OperClass и OperClassDto
            config.NewConfig<OperClass, OperClassDto>().PreserveReference(true);
            config.NewConfig<OperClassDto, OperClass>().PreserveReference(true);
        }

        // Метод для глобальной настройки Mapster
        // Применяет конфигурацию ко всем глобальным настройкам маппинга
        public static void ConfigureGlobal()
        {
            var config = TypeAdapterConfig.GlobalSettings; // Получаем глобальные настройки Mapster
            Configure(config); // Применяем локальную конфигурацию к глобальной
        }
    }
}
