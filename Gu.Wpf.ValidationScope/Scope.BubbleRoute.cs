namespace Gu.Wpf.ValidationScope;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>Implementation.</summary>
public static partial class Scope
{
    private static void UpdateParent(this DependencyObject source, IEnumerable<ValidationError> removed, IEnumerable<ValidationError> added)
    {
        UIElement? parent = null;
        if (source is DataGridRow row)
        {
            parent = GetDataGrid(row);
        }
        else
        {
            parent = VisualTreeHelper.GetParent(source) as UIElement;
        }

        if (parent == null || GetForInputTypes(parent) == null)
        {
            return;
        }

#pragma warning disable IDISP001, CA2000 // Dispose created. Disposed in SetNode.
        var parentNode = GetNode(parent) as ErrorNode ?? new ScopeNode(parent);
#pragma warning restore IDISP001, CA2000 // Dispose created.

        if (GetNode(source) is ErrorNode childNode)
        {
            _ = parentNode.ChildCollection.TryAdd(childNode);
        }

        parentNode.ErrorCollection.Remove(removed);
        parentNode.ErrorCollection.Add(added.Where(e => parent.IsScopeFor(e)).AsReadOnly());

        if (parentNode is ScopeNode { Errors.Count: 0 })
        {
            SetNode(parent, ValidNode.Default);
        }
        else
        {
            if (!ReferenceEquals(GetNode(parent), parentNode))
            {
                SetNode(parent, parentNode);
            }
        }
    }

    private static bool IsScopeFor(this DependencyObject parent, ValidationError error)
    {
        if (parent is UIElement element &&
            GetForInputTypes(element) is { } inputTypes)
        {
            foreach (var inputType in inputTypes)
            {
                if (inputType == typeof(Scope))
                {
                    return true;
                }

                if (inputType.IsInstanceOfType(error.Target()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsScopeFor(this DependencyObject parent, DependencyObject source)
    {
        if (parent is null || source is null)
        {
            return false;
        }

        if (parent is UIElement element &&
            GetForInputTypes(element) is { } inputTypes &&
            GetNode(source) is { } node)
        {
            if (node is ValidNode || node.Errors.Count == 0)
            {
                return false;
            }

            if (inputTypes.Contains(typeof(Scope)) && node is ScopeNode)
            {
                return true;
            }

            foreach (var error in GetErrors(source))
            {
                if (inputTypes.Contains(error.Target()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static DependencyObject Target(this ValidationError error)
    {
        return error switch
        {
            { BindingInError: null } => throw new ArgumentNullException(nameof(error), "error.BindingInError == null"),
            { BindingInError: BindingExpressionBase bindingExpression } => bindingExpression.Target,
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, $"ValidationError.BindingInError == {error.BindingInError}"),
        };
    }

    private static DataGrid? GetDataGrid(DataGridRow row)
    {
        var propertyInfo = typeof(DataGridRow).GetProperty("DataGridOwner", BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo == null)
        {
            return null;
        }

        return propertyInfo.GetValue(row) as DataGrid;
    }
}
