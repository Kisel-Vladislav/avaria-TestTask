using Ashsvp;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class LocationInstaller : MonoInstaller
    {
        [SerializeField] GameObject playerCarPrefab;
        [SerializeField] Transform carInitialPoint;
        public override void InstallBindings()
        {
            InitialPlayerCar();
        }

        private void InitialPlayerCar()
        {
            var playerCarController = Container.InstantiatePrefabForComponent<SimcadeVehicleController>(playerCarPrefab, carInitialPoint.position, carInitialPoint.rotation, null);

            Container.Bind<SimcadeVehicleController>()
                         .FromInstance(playerCarController)
                         .AsSingle();
        }
    }
}
