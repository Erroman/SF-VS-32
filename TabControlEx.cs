using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using static Constants;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
public class TabControlEx : TabControl
{
    private Panel ItemsHolderPanel = null;

    public TabControlEx()
        : base()
    {
        // This is necessary so that we get the initial databound selected item
        ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
    }

    /// <summary>
    /// If containers are done, generate the selected item
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    {
        if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            this.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            UpdateSelectedItem();
        }
    }

    /// <summary>
    /// Get the ItemsHolder and generate any children
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ItemsHolderPanel = GetTemplateChild("PART_ItemsHolder") as Panel;
        UpdateSelectedItem();
    }

    /// <summary>
    /// When the items change we remove any generated panel children and add any new ones as necessary
    /// </summary>
    /// <param name="e"></param>
    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (ItemsHolderPanel == null)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                ItemsHolderPanel.Children.Clear();
                break;

            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        ContentPresenter cp = FindChildContentPresenter(item);
                        if (cp != null)
                            ItemsHolderPanel.Children.Remove(cp);
                    }
                }

                // Don't do anything with new items because we don't want to
                // create visuals that aren't being shown

                UpdateSelectedItem();
                break;

            case NotifyCollectionChangedAction.Replace:
                throw new NotImplementedException("Replace not implemented yet");
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        UpdateSelectedItem();
    }

    private void UpdateSelectedItem()
    {
        if (ItemsHolderPanel == null)
            return;

        // Generate a ContentPresenter if necessary
        TabItem item = GetSelectedTabItem();
        if (item != null)
            CreateChildContentPresenter(item);

        // show the right child
        foreach (ContentPresenter child in ItemsHolderPanel.Children)
            child.Visibility = ((child.Tag as TabItem).IsSelected) ? Visibility.Visible : Visibility.Collapsed;
    }

    private ContentPresenter CreateChildContentPresenter(object item)
    {
        if (item == null)
            return null;

        ContentPresenter cp = FindChildContentPresenter(item);

        if (cp != null)
            return cp;

        // the actual child to be added.  cp.Tag is a reference to the TabItem
        cp = new ContentPresenter();
        cp.Content = (item is TabItem) ? (item as TabItem).Content : item;
        cp.ContentTemplate = this.SelectedContentTemplate;
        cp.ContentTemplateSelector = this.SelectedContentTemplateSelector;
        cp.ContentStringFormat = this.SelectedContentStringFormat;
        cp.Visibility = Visibility.Collapsed;
        cp.Tag = (item is TabItem) ? item : (this.ItemContainerGenerator.ContainerFromItem(item));
        ItemsHolderPanel.Children.Add(cp);
        return cp;
    }

    private ContentPresenter FindChildContentPresenter(object data)
    {
        if (data is TabItem)
            data = (data as TabItem).Content;

        if (data == null)
            return null;

        if (ItemsHolderPanel == null)
            return null;

        foreach (ContentPresenter cp in ItemsHolderPanel.Children)
        {
            if (cp.Content == data)
                return cp;
        }

        return null;
    }

    protected TabItem GetSelectedTabItem()
    {
        object selectedItem = base.SelectedItem;
        if (selectedItem == null)
            return null;

        TabItem item = selectedItem as TabItem;
        if (item == null)
            item = base.ItemContainerGenerator.ContainerFromIndex(base.SelectedIndex) as TabItem;

        return item;
    }
}
public static class VisualExtensions
{
    public static T GetVisualChild<T>(this Visual referenceVisual) where T : Visual
    {
        Visual child = null;
        for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceVisual); i++)
        {
            child = VisualTreeHelper.GetChild(referenceVisual, i) as Visual;
            if (child != null && child is T)
            {
                break;
            }
            else if (child != null)
            {
                child = GetVisualChild<T>(child);
                if (child != null && child is T)
                {
                    break;
                }
            }
        }
        return child as T;
    }
}