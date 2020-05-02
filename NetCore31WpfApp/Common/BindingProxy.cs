using System.Windows;

namespace MARGO.Common
{
    /* http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
      
       <DataGrid.Resources>
            <local:BindingProxy x:Key="proxy" Data="{Binding}" />
       </DataGrid.Resources>
     
        <DataGridTextColumn Header="Price" Binding="{Binding Price}" IsReadOnly="False"
                    Visibility="{Binding Data.ShowPrice,
                        Converter={StaticResource visibilityConverter},
                        Source={StaticResource proxy}}"/>
    */
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
