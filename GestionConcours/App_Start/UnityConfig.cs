using GestionConcours.Services;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace GestionConcours
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<ISearch3Service, Search3ServiceImp>();
            container.RegisterType<ICorbeil3Service, Corbeil3ServiceImp>();
            container.RegisterType<ISelectionService,SelectionServiceImp>();
            container.RegisterType<IPreselectionService, PreselectionServiceImp>();

			container.RegisterType<ICorrectionService, CorrectionServiceImp>();

			DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            container.RegisterType<IIndexService, IndexServiceImp>();
            container.RegisterType<IEpreuveService, EpreuveServicceImp>();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

        }
    }
}