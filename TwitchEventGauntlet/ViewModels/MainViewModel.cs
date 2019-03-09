using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.ViewModels
{
    class MainViewModel : Conductor<object>
    {
        private Task task;

        public async void LoadFakerPage()
        {
            await Task.Factory.StartNew(() =>
            {
                if (task != null)
                {
                    Task.WaitAny(task);
                }
                task = Task.Factory.StartNew(() => { ActivateItem(new StreamerViewModel("Mistafaker")); });
            });
        }
        public async void LoadMelPage()
        {
            await Task.Factory.StartNew(() =>
            {
                if (task != null)
                {
                    Task.WaitAny(task);
                }
                task = Task.Factory.StartNew(() => { ActivateItem(new StreamerViewModel("Melharucos")); });
            });
        }
        public async void LoadBjornPage()
        {
            await Task.Factory.StartNew(() =>
            {
                if (task != null)
                {
                    Task.WaitAny(task);
                }
                task = Task.Factory.StartNew(() => { ActivateItem(new StreamerViewModel("UncleBjorn")); });
            });
        }
        public async void LoadLasqaPage()
        {
            await Task.Factory.StartNew(() =>
            {
                if (task != null)
                {
                    Task.WaitAny(task);
                }
                task = Task.Factory.StartNew(() => { ActivateItem(new StreamerViewModel("Lasqa")); });
            });
        }
    }
}
