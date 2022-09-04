using System;
using System.ComponentModel;
using System.Windows.Input;

namespace WpfApp1
{
    public class UpdateStringValueCommand : ICommand
    {
        public UpdateStringValueCommand(MainVM mainVM)
        {
            this.mainVM = mainVM;
        }
        public bool CanExecute(object parameter)
        {
            this.mainVM.BoolVal = !this.mainVM.BoolVal;
            return true;
        }

        public event EventHandler CanExecuteChanged;
        private MainVM mainVM;

        public void Execute(object parameter)
        {
            this.mainVM.StringValue = "NewValue";
        }
    }
    public class MainVM : INotifyPropertyChanged
    {
        private string stringValue = "s";
        public string StringValue
        {
            get
            {
                return this.stringValue;
            }
            set
            {
                this.stringValue = value;
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StringValue"));
            }
        }

        private bool boolVal = false;
        public bool BoolVal
        {
            get
            {
                return this.boolVal;
            }
            set
            {
                this.boolVal = value;
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BoolVal"));
            }
        }

        public MainVM()
        {
            this.UpdateStringValue = new UpdateStringValueCommand(this);
        }
        public ICommand UpdateStringValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
