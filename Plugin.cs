using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WholeTomatoSoftware.SourceLinks;

/*
 * Source Links Example Plugin
 * This code is provided as is, without warranty of any kind.
 * 
 * Source Links plugins use a file extension of .slp.
 * 
 * To deploy this plugin for testing:
 *		- set the project debug action in the project properties: 
 *			- Project | SourceLinksExamplePlugin Properties | Debug | Start action
 *			- select "Start external program"
 *			- enter the path to a local installation of Visual Studio.  For example:
 *					C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe
 *		- build the project
 *		- copy SourceLinksExamplePlugin.slp from the output directory to 
 *			<Visual Assist installation directory>\SourceLinks
 *			To find your Visual Assist installation directory, see the following: 
 *			https://support.wholetomato.com/default.asp?W105
 *		- Press F5 to start an instance of Visual Studio, and in it go to:
 *			VassistX | Visual Assist Options... | Source Links
 *			to add or edit a link definition and select the built plugin from the drop-down list
 * 
 * To see trace messages at runtime, compile the Debug configuration.  Then either:
 *		- use the Output window in Visual Studio while debugging another instance of Visual Studio
 *	or
 *		- run DebugView (available from http://technet.microsoft.com/en-us/sysinternals/bb896647.aspx)
 * 
 * When it comes time for distribution, install your plugin to its own directory and 
 * add the directory to the PluginPaths registry value: 
 *		HKCU\Software\Whole Tomato\Visual Assist X\<IDE spec>\SourceLinks\PluginPaths
 * This string value contains a semicolon-delimited list of plugin paths.
 * Source Links looks for plugins in the specified paths as well as their sub-directories. 
 * Aside from testing and development, do not install to the Visual Assist installation 
 * directory as it is deleted when Visual Assist is updated.
 * 
 * If PluginPaths points to the target path of your plugin project, then re-build may 
 * fail if your IDE already has your existing .slp loaded.  In that case, before 
 * re-building it, you must close the IDE (to unlock the file), delete the existing .slp, 
 * restart the IDE and then build. 
 * 
 * If your plugin and/or factory will use the provided SettingsStore, generate new
 * Guids for the plugin and/or factory.
 * see:#generate_new_guid_factory and see:#generate_new_guid_plugin.
 * (Use Visual Assist Goto, or alt+g, to navigate to those VA Hashtags.)
 * 
 */


namespace SourceLinksExamplePlugin
{
	// Source Links loads your IPluginFactory implementation when Source Links initializes.
	// #generate_new_guid_factory : Generate a new Guid and replace the one below with it.
	[Guid("8bb807e9-35c6-41e8-bc12-8ef06635e3a0")]
	public class MyPluginFactory : IPluginFactory
	{
		public MyPluginFactory()
		{
			// Do common initialization that is applicable to all of your plugin 
			// instances created by this factory here.

			Debug.WriteLine("Source Links instantiated plugin factory");

			// Read factory settings from disk
			LoadSettings();
		}

		public string Name
		{
			// Provide a unique name to appear in the plugin dropdown on the Source Links page of 
			// the Visual Assist Options dialog.
			// The name is used to associate the plugin with link definitions created by the user.
			get
			{
				Debug.WriteLine("Source Links getting Name property");

				return "My Source Links Plugin (example project)";
			}
		}

		public IPlugin CreatePlugin(Guid linkId)
		{
			// CreatePlugin is called whenever a plugin instance is needed for the link definition specified by linkId.
			// A plugin is requested in the following circumstances (if not already cached by Source Links):
			// - when a user selects this plugin from the dropdown in the options dialog
			// - when a user hovers over an associated link instance in the text editor
			// - when a user double-clicks an associated link instance in the text editor

			// You are not responsible for disposing plugin instances, nor the plugin factory instance.
			// Always create new plugin instances in CreatePlugin; don't return cached instances from this method.
			// If your factory is requested to create a new plugin instance with a linkId that was previously 
			// specified, then the previous instance of the plugin has been, or will shortly be, disposed. 

			// You can return any new IPlugin implementation instance; they don't all have to be 
			// of the same derived type. 

			Debug.WriteLine("Source Links called CreatePlugin for link " + linkId.ToString());

			return new MyPlugin(this, linkId);
		}

		public void Dispose()
		{
			// Provide common cleanup here.
			// The Dispose method of IPluginFactory is called on shutdown of IDE.

			Debug.WriteLine("Source Links disposed plugin factory");

			// Write factory settings to disk
			SaveSettings();
		}

		public bool ShouldDoSomething { get; private set; }

		private void LoadSettings()
		{
			// load plugin factory settings (global to all instances of this plugin)
			// settings storage is dependent upon a stable Guid for MyPluginFactory; see:#generate_new_guid_factory
			using (var factorySettings = PluginUtils.GetSettingsStore<MyPluginFactory>())
			{
				ShouldDoSomething = factorySettings.GetValueOrDefaultAs<bool>("should_do_something", true);
			}
		}

		private void SaveSettings()
		{
			// load plugin factory settings (global to all instances of this plugin)
			// settings storage is dependent upon a stable Guid for MyPluginFactory; see:#generate_new_guid_factory
			using (var factorySettings = PluginUtils.GetSettingsStore<MyPluginFactory>())
			{
				factorySettings["should_do_something"] = ShouldDoSomething.ToString();
			}
		}
	}

	// Your IPluginFactory creates IPlugin implementation instances for Source Links.
	// An IPlugin instance is created for each link definition created by Source Links users.
	// #generate_new_guid_plugin : Generate a new Guid and replace the one below with it.
	[Guid("0d81a919-c5ba-4e7f-9984-7dc816087ae1")]
	public class MyPlugin : IPlugin
	{
		public MyPlugin(MyPluginFactory factory, Guid linkId)
		{
			// Local initialization of plugin instance for specified linkId

			// Store the factory to access shared settings
			_factory = factory;

			// Store the ID to be able to load/store settings/configurations per-link definition, use the 
			// link Guid to identify independent settings/configurations.
			_linkId = linkId;

			// Read stored settings from disk
			LoadSettings();
		}

		public PluginFlags Flags
		{
			// This property indicates to Source Links what behaviors the plugin implements.
			// Provide OR-ed values of the PluginFlags enumeration. 

			// Include PluginFlags.OverrideExecute to control link execution. 
			// Include PluginFlags.OverrideUrl to provide your own URL to be opened in the web browser.
			//		PluginFlags.OverrideUrl and PluginFlags.OverrideExecute are not applicable together. 
			//		PluginFlags.OverrideExecute is checked first; if it is set, PluginFlags.OverrideUrl is ignored. 
			// Include PluginFlags.OverrideRegex to define a regular expression used by Source Links to identify links in the text editor.
			// Include PluginFlags.Configurable to provide a configuration GUI.
			// Include PluginFlags.DisableQuickInfoMerging to prevent your tooltip text from being merged into Visual Studio Quick Info tooltips.
			// Include PluginFlags.DisableTooltipTheming if your tooltip text uses hard-coded colors.
			//		Only applicable if PluginFlags.DisableQuickInfoMerging is also specified.
			// Include PluginFlags.GetTooltipInUIThread to force GetTooltip(MarkerContext) to be called from the UI thread.
			//		By default, GetTooltip(MarkerContext) is called from a background thread so as to not block the UI thread.

			get
			{
				Debug.WriteLine("Source Links getting Flags property");
				return
					PluginFlags.Configurable |  // we want Configure() to get called 
					PluginFlags.OverrideUrl;    // we want GetUrl(MarkerContext) to get called
			}
		}

		public void Configure()
		{
			// Implement your configuration GUI here as a modal dialog.
			// For example, you might give user the ability to modify the default url.
			Debug.WriteLine("Source Links calling Configure()");

			// _url = ...;
			// _option = ...;

			// Write settings to disk
			SaveSettings();
		}

		public void Dispose()
		{
			// Provide cleanup for the plugin instance here.
			// The Dispose method is also called when a plugin is unassociated with a link definition.

			Debug.WriteLine("Source Links disposed plugin for link " + _linkId.ToString());
		}

		public void Execute(MarkerContext ctx)
		{
			// A MarkerContext object contains state describing a text editor instance of a link definition.

			// Execute will not get called in this example because Flags in this implementation don't include 
			// PluginFlags.OverrideExecute.
			// If you include PluginFlags.OverrideExecute in Flags, Execute is called when the user double-clicks the 
			// link instance described by the MarkerContext. You are responsible for executing the action related to 
			// the link. 
			// It is possible that any member of ctx may be null, string.Empty or 0.

			Debug.WriteLine("Source Links calling Execute(MarkerContext)");
		}

		public void ContextExecute(MarkerContext ctx)
		{
			// NOTE: ContextExecute is not yet supported.
		
			// ContextExecute will not get called in this example because Flags in this implementation don't
			// include PluginFlags.OverrideContextExecute.
			// If you include PluginFlags.OverrideContextExecute in Flags, ContextExecute is called when 
			// the user right-clicks on a link marker.
			Debug.WriteLine("Source Links calling ContextExecute()");
		}

		public string GetTooltip(MarkerContext ctx)
		{
			// A MarkerContext object contains state describing a text editor instance of a link definition.

			// GetTooltip gets called when the mouse cursor hovers over plugin-associated link text.
			// It is possible that any member of ctx may be null, string.Empty or 0.

			// The returned string can be either plane text or XAML. 
			// XAML text must be valid in the context of the VASourceLinks.dll assembly, thus all
			// resources must be defined in a way which the XAML parser may resolve. 

			// Return null or String.Empty if you don't need to provide tooltip text from your plugin. 

			Debug.WriteLine("Source Links calling GetTooltip(MarkerContext)");

			// We can determine the link definition behavior from the MarkerContext.
			// When Exe != null, then behavior is set to ShellExecute. 
			// When Url != null, then behavior is set to open URL in Browser. 
			// The Url, Exe and ExeArgs members of the MarkerContext contain information 
			// from the link definition; if your plugin overrides URL or Execute, the values 
			// may be irrelevant. 

			// Note: In this example, GetUrl is always called because Flags include 
			// OverrideUrl; user can't select ShellExecute behavior with the default
			// implementation of the example plugin. 

			string actionText = string.Empty;
			string actionLabel = string.Empty;

			if (Flags.HasFlag(PluginFlags.OverrideExecute))
			{
				actionLabel = "Execute:";
				actionText = "Overridden by plugin";
			}
			else if (Flags.HasFlag(PluginFlags.OverrideUrl))
			{
				actionLabel = "Open URL:";
				actionText = GetUrl(ctx);
			}
			else
			{
				if (ctx.Url != null)        // Open URL in Browser
				{
					actionLabel = "Open URL:";
					actionText = ctx.Url;
				}
				else if (ctx.Exe != null)   // ShellExecute
				{
					actionLabel = "ShellExecute:";
					actionText = ctx.Exe;
					if (!string.IsNullOrEmpty(ctx.ExeArgs))
						actionText += " " + ctx.ExeArgs;
				}
			}

			// The return string can be either plain text or valid XAML. 
			// This example demonstrates how to turn simple markup into valid XAML.
			// The returned XAML defines a TextBlock. 
			string tooltip = string.Format(

				// We can use formatted markup text for FlowDocument.
				// See: https://msdn.microsoft.com/en-us/library/aa970909(v=vs.110).aspx and
				// https://docs.microsoft.com/en-us/dotnet/articles/framework/wpf/advanced/flow-document-overview

				"<Bold>File name:</Bold> {0}<LineBreak/>" +
				"<Bold>Project file name:</Bold> {1}<LineBreak/>" +
				"<Bold>Solution file name:</Bold> {2}<LineBreak/>" +
				"<Bold>Line:</Bold> {3}<LineBreak/>" +
				"<Bold>Column:</Bold> {4}<LineBreak/>" +
				"<Bold>Line text:</Bold> {5}<LineBreak/>" +
				"<Bold>Marker text:</Bold> {6}<LineBreak/>" +

				// Or we can use some HTML inline tags: H1-H6, B, STRONG, I, EM, U, DEL, INS, S, BR 
				// Those tags are replaced with appropriate XAML tags by calling PluginUtils.WrapMarkupInXAML.
				// HTML attributes are not supported.
				"<b>Keyword text:</b> {7}<br/>" +
				"<b>Value text:</b> {8}<br/>" +
				"<b>" + actionLabel + "</b> {9}",

				// Images are supported:
				// "<Image Source=\"path\\to\\image.jpg\"/>"

				// Use PluginUtils.EncodeXML in places where text could contain special XML characters.
				PluginUtils.EncodeXML(System.IO.Path.GetFileName(ctx.FileName)), // 0
				PluginUtils.EncodeXML(System.IO.Path.GetFileName(ctx.ProjectName)), // 1
				PluginUtils.EncodeXML(System.IO.Path.GetFileName(ctx.SolutionName)), // 2
				ctx.Line, // 3
				ctx.Column, // 4
				PluginUtils.EncodeXML(ctx.LineText), // 5
				PluginUtils.EncodeXML(ctx.MarkerText), // 6
				PluginUtils.EncodeXML(ctx.KeywordText), // 7
				PluginUtils.EncodeXML(ctx.ValueText), // 8
				PluginUtils.EncodeXML(actionText)); // 9

			// PluginUtils.WrapMarkupInXAML replaces selected HTML formatting tags 
			// with appropriate XAML tags (<b> becomes <Bold> and <br> becomes 
			// <LineBreak>) and wraps the result into a TextBlock.

			// Note: Because WrapMarkupInXAML wraps your input into a TextBlock, it
			// can contain ONLY tags derived from Inline. If your tooltip must contain 
			// Block, then don't use this method; format the whole XAML directly, 
			// including required namespaces. 
			return PluginUtils.WrapMarkupInXAML(tooltip);
		}

		public string GetUrl(MarkerContext ctx)
		{
			// A MarkerContext object contains state describing a text editor instance of a link definition.

			// GetUrl is called when Flags include PluginFlags.OverrideUrl, allowing you to specify a 
			// state-dependent URL to open when the user double-clicks the associated link text.
			// ctx is null when GetUrl has been called from the Source Links page of the Visual Assist Options
			// dialog; in such cases, return text relevant to the user or simply return String.Empty (for 
			// read-only display in the link definition).
			// When ctx is not null, you are responsible for returning either a valid URL or null/string.Empty. 
			// Source Links will not modify the returned URL; it will only validate and execute it.

			Debug.WriteLine("Source Links calling GetUrl(MarkerContext)");

			if (ctx == null)
			{
				// When the MarkerContext is null, GetUrl has been called from the Source Links page
				// of the Visual Assist Options dialog to get text to display in the URL field of the
				// link definition.
				return _url;
			}
			else
			{
				// Url to be executed, based on text editor value matched by the link definition.
				// This example implementation also calls GetUrl directly from GetTooltip, passing
				// the MarkerContext that it received.
				return _url.Replace("$(Value)", ctx.ValueText);
			}
		}

		public string GetRegex()
		{
			// GetRegex will not get called in this example because Flags in this implementation don't include 
			// PluginFlags.OverrideRegex.
			// If you include PluginFlags.OverrideRegex in Flags, GetRegex is called for the plugin to specify 
			// a regular expression used to create links in the text editor.
			// If PluginFlags.OverrideRegex is set, GetRegex is called after Configure.
			Debug.WriteLine("Source Links calling GetRegex()");

			return @"(?<Keyword>fooId|barId)\s*[:=]\s*(?<Value>\d+)";
		}

		private void LoadSettings()
		{
			// settings storage is dependent upon GuidAttribute for MyPlugin; see:#generate_new_guid_plugin
			using (var settings = PluginUtils.GetSettingsStore<MyPlugin>(_linkId))
			{
				// Read saved value, or use default
				_url = settings.GetValueOrDefault("url", "http://localhost/plugin.aspx?param=$(Value)");
				_option = settings.GetValueOrDefaultAs<EnumSetting>("option", EnumSetting.OptionA);
			}
		}

		private void SaveSettings()
		{
			// settings storage is dependent upon a stable Guid for MyPlugin; see:#generate_new_guid_plugin
			using (var settings = PluginUtils.GetSettingsStore<MyPlugin>(_linkId))
			{
				settings["url"] = _url;
				settings["option"] = _option.ToString();
			}
		}

		private MyPluginFactory _factory;
		private Guid _linkId;

		// serialized settings
		private string _url;

		public enum EnumSetting
		{
			OptionA,
			OptionB,
			OptionC
		}
		private EnumSetting _option;
	}
}
