using Bindito.Core;
using MigrationControl.UI;
using Timberborn.EntityPanelSystem;
using Timberborn.GameDistricts;
using Timberborn.TemplateInstantiation;

namespace MigrationControl
{
    [Context("Game")]
    internal class MigrationControlConfigurator : Configurator
    {
        protected override void Configure()
        {
            Bind<MigrationFilters>().AsTransient();
            Bind<MigrationPin>().AsTransient();
            Bind<MigrationNotifier>().AsSingleton();
            Bind<MigrationFiltersFragment>().AsSingleton();
            Bind<MigrationPinFragment>().AsSingleton();
            MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
            MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
        }

        private static TemplateModule ProvideTemplateModule()
        {
            var builder = new TemplateModule.Builder();
            builder.AddDecorator<DistrictCenter, MigrationFilters>();
            // Citizen is on every beaver (adult and child) and every bot.
            builder.AddDecorator<Citizen, MigrationPin>();
            return builder.Build();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            private readonly MigrationFiltersFragment _filtersFragment;
            private readonly MigrationPinFragment _pinFragment;

            public EntityPanelModuleProvider(MigrationFiltersFragment filtersFragment, MigrationPinFragment pinFragment)
            {
                _filtersFragment = filtersFragment;
                _pinFragment = pinFragment;
            }

            public EntityPanelModule Get()
            {
                var builder = new EntityPanelModule.Builder();
                builder.AddMiddleFragment(_filtersFragment, 50);
                builder.AddMiddleFragment(_pinFragment, 50);
                return builder.Build();
            }
        }
    }
}
