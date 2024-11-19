using _Project.CodeBase.Ghost;
using _Project.CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Service.InputService;
using UnityEngine.SceneManagement;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller, ICoroutineRunner
    {
        public override void InstallBindings()
        {
            BindingInputService();
            BindingCoroutineRunner();
            BindingReplayService();

            LoadGameScene();
        }

        private void BindingCoroutineRunner() =>
            Container.Bind<ICoroutineRunner>()
                     .FromInstance(this)
                     .AsSingle();

        private void BindingReplayService() =>
            Container.BindInterfacesAndSelfTo<ReplaySystem>()
                     .AsSingle();

        private void BindingInputService() =>
            Container.Bind<IInputService>()
                     .To<InputSystemService>()
                     .AsSingle()
                     .NonLazy();

        public void LoadGameScene() => 
            SceneManager.LoadScene("Game");
    }
}