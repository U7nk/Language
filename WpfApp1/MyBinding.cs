using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace WpfApp1;

internal class MyBinding : MarkupExtension
{
    public string Value { get; set; }
    INotifyPropertyChanged _context;
    DependencyObject _target;
    DependencyProperty _targetProperty;
    Dictionary<string, object> _contextAvailableMembers;
    Func<object> CompiledBinding { get; set; }
    public MyBinding(string value)
    {
        Value = value;
        _contextAvailableMembers = new Dictionary<string, object>();
    }
        
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
       
            
        return CompiledBinding();
    }

    void InitializeContext(IServiceProvider serviceProvider)
    {
        IProvideValueTarget provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
        var targetObject = provideValueTarget.TargetObject;
        _targetProperty = (DependencyProperty)provideValueTarget.TargetProperty;
        if (targetObject is FrameworkElement)
        {
            var frameworkElement = (FrameworkElement)targetObject;
            _target = frameworkElement;
            if (frameworkElement.DataContext is INotifyPropertyChanged)
            {
                _context = (INotifyPropertyChanged)frameworkElement.DataContext;
                _context.PropertyChanged += (s, e) => { _target.SetValue(_targetProperty, ProvideValue(serviceProvider)); };
                _contextAvailableMembers["$ctx"] = _context;
            }
        }
    }
}