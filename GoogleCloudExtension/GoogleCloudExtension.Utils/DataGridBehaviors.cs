﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GoogleCloudExtension.Utils
{
    public static class DataGridBehaviors
    {
        #region HasCustomSort property.

        public static readonly DependencyProperty HasCustomSortProperty =
            DependencyProperty.RegisterAttached(
                "HasCustomSort",
                typeof(bool),
                typeof(DataGridBehaviors),
                new PropertyMetadata(false, OnHasCustomSortPropertyChanged));

        public static bool GetHasCustomSort(DataGrid self) => (bool)self.GetValue(HasCustomSortProperty);

        public static void SetHasCustomSort(DataGrid self, bool value)
        {
            self.SetValue(HasCustomSortProperty, value);
        }

        private static void OnHasCustomSortPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as DataGrid;
            if (self == null)
            {
                Debug.WriteLine($"Attempted to use {nameof(HasCustomSortProperty)} on type {d.GetType().Name}");
                return;
            }

            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;

            if (oldValue)
            {
                self.Sorting -= OnDataGridSorting;
            }

            if (newValue)
            {
                self.Sorting += OnDataGridSorting;
            }
        }

        #endregion

        #region CustomSort property.

        public static readonly DependencyProperty CustomSortProperty =
            DependencyProperty.RegisterAttached(
                "CustomSort",
                typeof(IColumnSorter),
                typeof(DataGridBehaviors));

        public static IColumnSorter GetCustomSort(DataGridColumn self) => (IColumnSorter)self.GetValue(CustomSortProperty);

        public static void SetCustomSort(DataGridColumn self, IColumnSorter value)
        {
            self.SetValue(CustomSortProperty, value);
        }

        #endregion

        private static void OnDataGridSorting(object sender, DataGridSortingEventArgs e)
        {
            var self = (DataGrid)sender;
            var column = e.Column;

            var customSorter = GetCustomSort(column);
            if (customSorter == null)
            {
                // No custom sort is defined for this column, revert to the built-in sorting.
                return;
            }

            var oldIsDescending = column.SortDirection == System.ComponentModel.ListSortDirection.Descending;
            var newIsDescending = !oldIsDescending;

            var collectionView = self.ItemsSource as ListCollectionView;
            if (collectionView == null)
            {
                Debug.WriteLine($"Was unable to find collection view, found {self.ItemsSource?.GetType().Name}");
                return;
            }

            column.SortDirection = newIsDescending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending;
            collectionView.CustomSort = new DataGridColumnCustomSorter(customSorter, newIsDescending);

            e.Handled = true;   
        }
    }
}
