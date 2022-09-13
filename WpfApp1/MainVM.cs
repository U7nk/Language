using System;
using System.ComponentModel;
using System.Windows.Input;

namespace WpfApp1
{
    public class UpdateStringValueCommand : ICommand
    {
        public UpdateStringValueCommand(MainVm mainVm)
        {
            this._mainVm = mainVm;
        }
        public bool CanExecute(object parameter)
        {
            _mainVm.BoolVal = !_mainVm.BoolVal;
            return true;
        }

        public event EventHandler CanExecuteChanged;
        MainVm _mainVm;

        public void Execute(object parameter)
        {
            _mainVm.StringValue = "NewValue";
        }
    }
    public class MainVm : INotifyPropertyChanged
    {
        string _stringValue = "s";
        public string StringValue
        {
            get
            {
                return _stringValue;
            }
            set
            {
                _stringValue = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StringValue"));
            }
        }

        bool _boolVal = false;
        public bool BoolVal
        {
            get
            {
                return _boolVal;
            }
            set
            {
                _boolVal = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BoolVal"));
            }
        }

        public MainVm()
        {
            UpdateStringValue = new UpdateStringValueCommand(this);
        }
        public ICommand UpdateStringValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
