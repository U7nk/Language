using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.ComponentModel;
using Wired;
using Wired.Compilers;

namespace WPFApp
{
    class MyBinding : MarkupExtension
    {
        public string Value { get; set; }
        private INotifyPropertyChanged context;
        private DependencyObject target;
        private DependencyProperty targetProperty;
        private Dictionary<string, object> contextAvailableMembers;
        private Func<object> CompiledBinding { get; set; }
        public MyBinding(string value)
        {
            Value = value;
            contextAvailableMembers = new Dictionary<string, object>();
        }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (context == null)
            {
                InitializeContext(serviceProvider);
            }

            if (CompiledBinding == null)
            {
                var tokens = new Tokenizer(Value).Tokenize();
                var ast = new WiredParser(context.GetType()).Parse(tokens);
                new TypeResolver(AppDomain.CurrentDomain, new []{ "System.Windows" }).Resolve(ref ast, context.GetType());
                CompiledBinding = new WiredCompiler(contextAvailableMembers).Compile(ast);
            }
            
            return CompiledBinding();
        }
        
        private void InitializeContext(IServiceProvider serviceProvider)
        {
            IProvideValueTarget provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetObject = provideValueTarget.TargetObject;
            targetProperty = (DependencyProperty)provideValueTarget.TargetProperty;
            if (targetObject is FrameworkElement)
            {
                var frameworkElement = (FrameworkElement)targetObject;
                target = frameworkElement;
                if (frameworkElement.DataContext is INotifyPropertyChanged)
                {
                    context = (INotifyPropertyChanged)frameworkElement.DataContext;
                    context.PropertyChanged += (s, e) => { target.SetValue(targetProperty, ProvideValue(serviceProvider)); };
                    contextAvailableMembers["$ctx"] = context;
                }
            }
        }
    }
}
