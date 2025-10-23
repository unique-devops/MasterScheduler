using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Service
{
    public static class LoaderService
    {
        private static Action _showAction;
        private static Action _hideAction;

        public static void Register(Action show, Action hide)
        {
            _showAction = show;
            _hideAction = hide;
        }

        public static void ShowLoader() => _showAction?.Invoke();
        public static void HideLoader() => _hideAction?.Invoke();
    }
}
