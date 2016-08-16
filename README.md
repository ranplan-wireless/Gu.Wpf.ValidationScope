# Gu.Wpf.ValidationScope

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Gitter](https://badges.gitter.im/JohanLarsson/Gu.Wpf.ValidationScope.svg)](https://gitter.im/JohanLarsson/Gu.Wpf.ValidationScope?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![NuGet](https://img.shields.io/nuget/v/Gu.Wpf.ValidationScope.svg)](https://www.nuget.org/packages/Gu.Wpf.ValidationScope/)
[![Build status](https://ci.appveyor.com/api/projects/status/omv9baijykp70dfr?svg=true)](https://ci.appveyor.com/project/JohanLarsson/gu-wpf-validationscope)

The samples assumes an xml namespace alias `xmlns:validation="https://github.com/JohanLarsson/Gu.Wpf.ValidationScope"` is defined.

## Sample:
```xaml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <Border BorderBrush="{Binding Path=(validation:Scope.HasError),
                                  Converter={local:BoolToBrushConverter},
                                  ElementName=Form}"
            BorderThickness="1">
        <Grid x:Name="Form"
                validation:Scope.ForInputTypes="{x:Static validation:InputTypeCollection.Default}">
                <!-- this is where we define our scope, we do so by telling the scope what types of control sto track -->
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition />
            </Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition />
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Row="0"
                       Grid.Column="0"
                       Text="SomeIntValue" />
            <TextBox Grid.Row="0"
                     Grid.Column="1"
                     Text="{Binding IntValue,
                                    UpdateSourceTrigger=PropertyChanged}" />
            <TextBlock Grid.Row="1"
                       Grid.Column="0"
                       Text="SameIntValue" />
            <TextBox Grid.Row="1"
                     Grid.Column="1"
                     Text="{Binding IntValue,
                     UpdateSourceTrigger=PropertyChanged}" />
        </Grid>
    </Border>

    <ItemsControl Grid.Row="1"
                  ItemsSource="{Binding Path=(validation:Scope.Errors),
                                        ElementName=Form}">
        <ItemsControl.ItemTemplate>
            <DataTemplate DataType="{x:Type ValidationError}">
                <TextBlock Foreground="Red"
                           Text="{Binding ErrorContent}" />
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Grid>
```
More samples in the demo project

Renders:

![ItemsSource2D render](http://i.imgur.com/EkuWA9c.gif)

##### Defining a ValidationScope
```xaml
<Border validation:Scope.ForInputTypes="TextBox, Slider">
    <StackPanel>
        <!--The stackpanel will inherit the scope-->
        <TextBox Text="{Binding Value1}" />
        <TextBox Text="{Binding Value2}" />
        <ComboBox IsEditable="True" Text="{Binding Value3}" />
        <!-- this combobox will not be tracked because the scope is only for textboxes and sliders--> 
    </StackPanel>
</Border>
```

You can also use `<Border validation:Scope.ForInputTypes="{x:Static validation:InputTypeCollection.Default}">`

##### InputTypeCollection.Default contains:
typeof(Scope),
typeof(TextBoxBase),
typeof(ComboBox),
typeof(ToggleButton),
typeof(Slider)
And should be enough for most scenarios when you don't have third party controls for example a third party textbox that does not derive from `TextBoxBase`

##### InputTypes markupextension
`<Border validation:Scope.ForInputTypes="{validation:InputTypes {x:Type TextBox}, {x:Type ComboBox}}">`
Exposed for convenience to create list of types in xaml.

##### If you need to bind HasError to DataContext there is a hack exposed:
```xaml
<Grid x:Name="Form"
        validation:Scope.ErrorsOneWayToSourceBinding="{Binding Errors,
                                                               Mode=OneWayToSource}"
        validation:Scope.ForInputTypes="{x:Static validation:InputTypeCollection.Default}"
        validation:Scope.HasErrorOneWayToSourceBinding="{Binding HasError,
                                                                 Mode=OneWayToSource}">
        ...
```

This is a workaround for WPF's limitation of not allowing OneWayToSource bindings of readonly dependency properties.
Updating the viewmodel property does nothing but changes in the view are sent to the viewmodel.
