//using System;
//using Zenject;

//namespace Raindrop
//{
//    class GameInstaller1 : MonoInstaller
//    {
//        public override void InstallBindings()
//        {
//            Container.Bind<IRaindropService>()
//                .To<RaindropInstance>()
//                .AsSingle();

//            Container.Bind<UIInstance>().AsSingle().NonLazy();
//            Container.Bind<RaindropInstance>().AsSingle().NonLazy();

//        }
//    }
//}
