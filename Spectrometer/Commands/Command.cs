using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spectrometer.Commands
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action action = null;

        public Command(Action action) => this.action = action;

        public Action Action
        {
            get => action;
            set => action = value;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Action?.Invoke();
        }
    }
}
