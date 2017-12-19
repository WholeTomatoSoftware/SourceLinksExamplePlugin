using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WholeTomatoSoftware.SourceLinks;

namespace SourceLinksExamplePlugin
{
	internal class ExampleContextMenu : ContextMenu
	{
		internal class ExampleContextMenuItem : MenuItem, ICommand
		{
			public ExampleContextMenuItem(MarkerContext ctx, string text)
			{
				_ctx = ctx;
				// Header text might be dependent upon state in ctx MarkerContext
				Header = text;
				Command = this;
				// Icon = new Image { Source = YourImage };
			}

			public bool CanExecute(object parameter)
			{
				// CanExecute might be dependent upon state in ctx MarkerContext
				return true;
			}

			public void Execute(object parameter)
			{
				Debug.WriteLine("ExampleContextMenuItem.Execute called");
				// Execute might be dependent upon state in ctx MarkerContext
			}

			public event EventHandler CanExecuteChanged;
			private MarkerContext _ctx;
		}

		internal ExampleContextMenu(MarkerContext ctx)
		{
			// Wire up theming of ContextMenu and MenuItem via Visual Assist VaWPFTheming.dll
			Resources.MergedDictionaries.Add(new ResourceDictionary()
			{
				Source = new Uri("pack://application:,,,/VaWPFTheming;component/ThemedControls.xaml", UriKind.Absolute)
			});
			Style = TryFindResource(typeof(ContextMenu)) as Style;

			// add context menu command items
			Items.Add(new ExampleContextMenuItem(ctx, "Command _1")
			{
				Style = TryFindResource(typeof(MenuItem)) as Style
			});

			Items.Add(new ExampleContextMenuItem(ctx, "Command _2")
			{
				Style = TryFindResource(typeof(MenuItem)) as Style
			});

			Items.Add(new Separator());

			Items.Add(new ExampleContextMenuItem(ctx, "Command _3")
			{
				Style = TryFindResource(typeof(MenuItem)) as Style
			});
		}

		protected override void OnOpened(RoutedEventArgs e)
		{
			base.OnOpened(e);
			if (Items.Count > 0)
				((MenuItem)Items[0]).Focus();
		}
	}
}
