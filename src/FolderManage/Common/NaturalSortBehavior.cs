using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FolderManage.Common;

/// <summary>
/// DataGrid 열 머리글을 눌러 정렬할 때 숫자를 숫자로 인식하게 하는 첨부 속성(<c>common:NaturalSortBehavior.IsEnabled="True"</c>).
/// 기본 정렬은 문자열 비교라 "10"이 "2"보다 앞에 오므로, 정렬을 <see cref="NaturalStringComparer"/>로 대신한다.
/// </summary>
public static class NaturalSortBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(NaturalSortBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not DataGrid grid)
        {
            return;
        }

        grid.Sorting -= OnSorting;
        if ((bool)e.NewValue)
        {
            grid.Sorting += OnSorting;
        }
    }

    private static void OnSorting(object? sender, DataGridSortingEventArgs e)
    {
        var grid = (DataGrid)sender!;
        var propertyName = e.Column.SortMemberPath;
        if (string.IsNullOrEmpty(propertyName) ||
            CollectionViewSource.GetDefaultView(grid.ItemsSource) is not ListCollectionView view)
        {
            return;
        }

        e.Handled = true;

        var direction = e.Column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        foreach (var column in grid.Columns)
        {
            column.SortDirection = null;
        }

        e.Column.SortDirection = direction;
        view.CustomSort = new PropertyNaturalComparer(propertyName, direction);
    }

    // 항목의 속성 하나를 문자열로 읽어 자연 정렬 순서로 비교한다.
    private sealed class PropertyNaturalComparer : IComparer
    {
        private readonly string _propertyName;
        private readonly int _sign;
        private PropertyInfo? _property;

        public PropertyNaturalComparer(string propertyName, ListSortDirection direction)
        {
            _propertyName = propertyName;
            _sign = direction == ListSortDirection.Ascending ? 1 : -1;
        }

        public int Compare(object? x, object? y)
        {
            return _sign * NaturalStringComparer.Instance.Compare(Read(x), Read(y));
        }

        private string? Read(object? item)
        {
            if (item is null)
            {
                return null;
            }

            _property ??= item.GetType().GetProperty(_propertyName);
            return _property?.GetValue(item)?.ToString();
        }
    }
}
