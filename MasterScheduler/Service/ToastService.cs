using MasterScheduler.Shared.Enums;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Service
{
    public class ToastService
    {
        private ToastMessage _toast;

        public void Register(ToastMessage toast)
        {
            _toast = toast;
        }

        public async Task ShowAsync(string message, ToastType type = ToastType.Info, int durationMs = 2500)
        {
            if (_toast != null)
                await _toast.ShowAsync(message, type, durationMs);
        }
    }
}
