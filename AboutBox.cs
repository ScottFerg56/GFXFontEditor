using Microsoft.Win32;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GFXFontEditor
{
	public partial class AboutBox : Form
	{
		public AboutBox()
		{
			InitializeComponent();
			Icon = Application.OpenForms[0]?.Icon;
		}

		private bool _IsPainted = false;
		private Assembly _EntryAssembly;
		private NameValueCollection _EntryAssemblyAttribCollection;

		// <summary>
		// returns the entry assembly for the current application domain
		// </summary>
		// <remarks>
		// This is usually read-only, but in some weird cases (Smart Client apps) 
		// you won't have an entry assembly, so you may want to set this manually.
		// </remarks>
		public Assembly AppEntryAssembly
		{
			get
			{
				return _EntryAssembly;
			}
			set
			{
				_EntryAssembly = value;
			}
		}

		// <summary>
		// single line of text to show in the application title section of the about box dialog
		// </summary>
		// <remarks>
		// defaults to "%title%" 
		// %title% = Assembly: AssemblyTitle
		// </remarks>
		public string AppTitle
		{
			get
			{
				return AppTitleLabel.Text;
			}
			set
			{
				AppTitleLabel.Text = value;
			}
		}

		// <summary>
		// single line of text to show in the description section of the about box dialog
		// </summary>
		// <remarks>
		// defaults to "%description%"
		// %description% = Assembly: AssemblyDescription
		// </remarks>
		public string AppDescription
		{
			get
			{
				return AppDescriptionLabel.Text;
			}
			set
			{
				if (value == ""){
					AppDescriptionLabel.Visible = false;
				}else{
					AppDescriptionLabel.Visible = true;
					AppDescriptionLabel.Text = value;
				}
			}
		}

		// <summary>
		// single line of text to show in the version section of the about dialog
		// </summary>
		// <remarks>
		// defaults to "Version %version%"
		// %version% = Assembly: AssemblyVersion
		// </remarks>
		public string AppVersion
		{
			get{
				return AppVersionLabel.Text;
			}
			set{
				if (value == ""){
					AppVersionLabel.Visible = false;
				}else{
					AppVersionLabel.Visible = true;
					AppVersionLabel.Text = value;
				}
			}
		}

		// <summary>
		// single line of text to show in the copyright section of the about dialog
		// </summary>
		// <remarks>
		// defaults to "Copyright � %year%, %company%"
		// %company% = Assembly: AssemblyCompany
		// %year% = current 4-digit year
		// </remarks>
		public string AppCopyright
		{
			get{
				return AppCopyrightLabel.Text;
			}
			set{
				if (value == ""){
					AppCopyrightLabel.Visible = false;
				}else{
					AppCopyrightLabel.Visible = true;
					AppCopyrightLabel.Text = value;
				}
			}
		}

		// <summary>
		// intended for the default 32x32 application icon to appear in the upper left of the about dialog
		// </summary>
		// <remarks>
		// if you open this form using .ShowDialog(Owner), the icon can be derived from the owning form
		// </remarks>
		public Image AppImage
		{
			get{
				return ImagePictureBox.Image;
			}
			set{
				ImagePictureBox.Image = value;
			}
		}

		// <summary>
		// multiple lines of miscellaneous text to show in rich text box
		// </summary>
		// <remarks>
		// defaults to "%product% is %copyright%, %trademark%"
		// %product% = Assembly: AssemblyProduct
		// %copyright% = Assembly: AssemblyCopyright
		// %trademark% = Assembly: AssemblyTrademark
		// </remarks>
		public string AppMoreInfo
		{
			get{
				return MoreRichTextBox.Text;
			}
			set{
				if (value == null || value == ""){
					MoreRichTextBox.Visible = false;
				}else{
					MoreRichTextBox.Visible = true;
					MoreRichTextBox.Text = value;
				}
			}
		}

		// <summary>
		// exception-safe retrieval of LastWriteTime for this assembly.
		// </summary>
		// <returns>File.GetLastWriteTime, or DateTime.MaxValue if exception was encountered.</returns>
		private static DateTime AssemblyLastWriteTime(Assembly a)
		{
			if (a.Location == null || a.Location == "")
				return DateTime.MaxValue;
			try{
				return File.GetLastWriteTime(a.Location);
			}catch(Exception){
				return DateTime.MaxValue;
			}
		}

		// <summary>
		// returns string name / string value pair of all attribs
		// for specified assembly
		// </summary>
		// <remarks>
		// note that Assembly* values are pulled from AssemblyInfo file in project folder
		//
		// Trademark       = AssemblyTrademark string
		// Debuggable      = true
		// GUID            = 7FDF68D5-8C6F-44C9-B391-117B5AFB5467
		// CLSCompliant    = true
		// Product         = AssemblyProduct string
		// Copyright       = AssemblyCopyright string
		// Company         = AssemblyCompany string
		// Description     = AssemblyDescription string
		// Title           = AssemblyTitle string
		// </remarks>
		private static NameValueCollection AssemblyAttribs(Assembly a)
		{
			string TypeName;
			string Name;
			string Value;
			NameValueCollection nvc = new();
			Regex r = new(@"(\.Assembly|\.)(?<Name>[^.]*)Attribute$", RegexOptions.IgnoreCase);

			foreach (object attrib in a.GetCustomAttributes(false))
			{
				TypeName = attrib.GetType().ToString();
				Name = r.Match(TypeName).Groups["Name"].ToString();
				Value = "";
				switch (TypeName)
				{
					case "System.CLSCompliantAttribute":
						Value = ((CLSCompliantAttribute)attrib).IsCompliant.ToString(); break;
					case "System.Diagnostics.DebuggableAttribute":
						Value = ((System.Diagnostics.DebuggableAttribute)attrib).IsJITTrackingEnabled.ToString(); break;
					case "System.Reflection.AssemblyCompanyAttribute":
						Value = ((AssemblyCompanyAttribute)attrib).Company.ToString(); break;
					case "System.Reflection.AssemblyConfigurationAttribute":
						Value = ((AssemblyConfigurationAttribute)attrib).Configuration.ToString(); break;
					case "System.Reflection.AssemblyCopyrightAttribute":
						Value = ((AssemblyCopyrightAttribute)attrib).Copyright.ToString(); break;
					case "System.Reflection.AssemblyDefaultAliasAttribute":
						Value = ((AssemblyDefaultAliasAttribute)attrib).DefaultAlias.ToString(); break;
					case "System.Reflection.AssemblyDelaySignAttribute":
						Value = ((AssemblyDelaySignAttribute)attrib).DelaySign.ToString(); break;
					case "System.Reflection.AssemblyDescriptionAttribute":
						Value = ((AssemblyDescriptionAttribute)attrib).Description.ToString(); break;
					case "System.Reflection.AssemblyInformationalVersionAttribute":
						Value = ((AssemblyInformationalVersionAttribute)attrib).InformationalVersion.ToString(); break;
					case "System.Reflection.AssemblyKeyFileAttribute":
						Value = ((AssemblyKeyFileAttribute)attrib).KeyFile.ToString(); break;
					case "System.Reflection.AssemblyProductAttribute":
						Value = ((AssemblyProductAttribute)attrib).Product.ToString(); break;
					case "System.Reflection.AssemblyTrademarkAttribute":
						Value = ((AssemblyTrademarkAttribute)attrib).Trademark.ToString(); break;
					case "System.Reflection.AssemblyTitleAttribute":
						Value = ((AssemblyTitleAttribute)attrib).Title.ToString(); break;
					case "System.Resources.NeutralResourcesLanguageAttribute":
						Value = ((System.Resources.NeutralResourcesLanguageAttribute)attrib).CultureName.ToString(); break;
					case "System.Resources.SatelliteContractVersionAttribute":
						Value = ((System.Resources.SatelliteContractVersionAttribute)attrib).Version.ToString(); break;
					case "System.Runtime.InteropServices.ComCompatibleVersionAttribute":
						{
							System.Runtime.InteropServices.ComCompatibleVersionAttribute x;
							x = ((System.Runtime.InteropServices.ComCompatibleVersionAttribute)attrib);
							Value = x.MajorVersion + "." + x.MinorVersion + "." + x.RevisionNumber + "." + x.BuildNumber; break;
						}
					case "System.Runtime.InteropServices.ComVisibleAttribute":
						Value = ((System.Runtime.InteropServices.ComVisibleAttribute)attrib).Value.ToString(); break;
					case "System.Runtime.InteropServices.GuidAttribute":
						Value = ((System.Runtime.InteropServices.GuidAttribute)attrib).Value.ToString(); break;
					case "System.Runtime.InteropServices.TypeLibVersionAttribute":
						{
							System.Runtime.InteropServices.TypeLibVersionAttribute x;
							x = ((System.Runtime.InteropServices.TypeLibVersionAttribute)attrib);
							Value = x.MajorVersion + "." + x.MinorVersion; break;
						}
					case "System.Security.AllowPartiallyTrustedCallersAttribute":
						Value = "(Present)"; break;
					default:
						// debug.writeline("** unknown assembly attribute '" + TypeName + "'")
						Value = TypeName; break;
				}

				if (nvc[Name] == null){
					nvc.Add(Name, Value);
				}
			}

			// build date
			DateTime dt = AssemblyLastWriteTime(a);
			if (dt == DateTime.MaxValue){
				nvc.Add("BuildDate", "(unknown)");
			}else{
				nvc.Add("BuildDate", dt.ToString("yyyy-MM-dd hh:mm tt"));
			}
			// location
			try{
				nvc.Add("Location", a.Location);
			}catch(NotSupportedException){
				nvc.Add("Location", "(not supported)");
			}
			// version
			try{
				if (a.GetName().Version.Major == 0 && a.GetName().Version.Minor == 0){
					nvc.Add("Version", "(unknown)");
				}else{
					nvc.Add("Version", a.GetName().Version.ToString());
				}
			}catch(Exception){
				nvc.Add("Version", "(unknown)");
			}

			nvc.Add("FullName", a.FullName);

			return nvc;
		}

		// <summary>
		// reads an HKLM Windows Registry key value
		// </summary>
		private static string RegistryHklmValue(string KeyName, string SubKeyRef)
		{
			RegistryKey rk;
			try{
				rk = Registry.LocalMachine.OpenSubKey(KeyName);
				return (string)rk.GetValue(SubKeyRef, "");
			}catch(Exception){
				return "";
			}
		}

		// <summary>
		// launch the MSInfo "system information" application (works on XP, 2003, and Vista)
		// </summary>
		private void ShowSysInfo()
		{
			string strSysInfoPath = RegistryHklmValue(@"SOFTWARE\Microsoft\Shared Tools Location", "MSINFO");
			if (strSysInfoPath == ""){
				strSysInfoPath = RegistryHklmValue(@"SOFTWARE\Microsoft\Shared Tools\MSINFO", "PATH");
			}

			if (strSysInfoPath == ""){
				MessageBox.Show("System Information is unavailable at this time." +
					Environment.NewLine +
					Environment.NewLine +
					"(couldn't find path for Microsoft System Information Tool in the registry.)",
					Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try{
				System.Diagnostics.Process.Start(strSysInfoPath);
			}catch(Exception){
				MessageBox.Show("System Information is unavailable at this time." +
					Environment.NewLine +
					Environment.NewLine +
					"(couldn't launch '" + strSysInfoPath + "')",
					Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}

		}

		// <summary>
		// retrieves a cached value from the entry assembly attribute lookup collection
		// </summary>
		private string EntryAssemblyAttrib(string strName)
		{
			if (_EntryAssemblyAttribCollection[strName] == null){
				return "<Assembly: Assembly" + strName + "(\"\")>";
			}else{
				return _EntryAssemblyAttribCollection[strName].ToString();
			}
		}

		// <summary>
		// Populate all the form labels with tokenized text
		// </summary>
		private void PopulateLabels()
		{
			FWVersionLabel.Text =
#if NET7_0       
			".NET 7.0";
#elif NET6_0
			".NET 6.0";
#elif NET5_0
			".NET 5.0";
#elif NET47
			".Net Framework 4.7";
#else
			"Unknown Framework";
			Debug.Fail("Unknown Framework");
#endif

			// get entry assembly attribs
			_EntryAssemblyAttribCollection = AssemblyAttribs(_EntryAssembly);

			// set icon from parent, if present
			if (Owner == null)
			{
				ImagePictureBox.Visible = false;
				AppTitleLabel.Left = AppCopyrightLabel.Left;
				AppDescriptionLabel.Left = AppCopyrightLabel.Left;
			}
			else
			{
				Icon = Owner.Icon;
				ImagePictureBox.Image = Icon.ToBitmap();
			}
			// replace all labels and window title
			Text = ReplaceTokens(Text);
			AppTitleLabel.Text = ReplaceTokens(AppTitleLabel.Text);
			if (AppDescriptionLabel.Visible){
				AppDescriptionLabel.Text = ReplaceTokens(AppDescriptionLabel.Text);
			}
			if (AppCopyrightLabel.Visible){
				AppCopyrightLabel.Text = ReplaceTokens(AppCopyrightLabel.Text);
			}
			if (AppVersionLabel.Visible){
				AppVersionLabel.Text = ReplaceTokens(AppVersionLabel.Text);
			}
			if (AppDateLabel.Visible){
				AppDateLabel.Text = ReplaceTokens(AppDateLabel.Text);
			}
			if (MoreRichTextBox.Visible){
				MoreRichTextBox.Text = ReplaceTokens(MoreRichTextBox.Text);
			}
		}

		// <summary>
		// perform assemblyinfo to string replacements on labels
		// </summary>
		private string ReplaceTokens(string s)
		{
			s = s.Replace("%title%", EntryAssemblyAttrib("title"));
			s = s.Replace("%copyright%", EntryAssemblyAttrib("copyright"));
			s = s.Replace("%description%", EntryAssemblyAttrib("description"));
			s = s.Replace("%company%", EntryAssemblyAttrib("company"));
			s = s.Replace("%product%", EntryAssemblyAttrib("product"));
			s = s.Replace("%trademark%", EntryAssemblyAttrib("trademark"));
			s = s.Replace("%year%", DateTime.Now.Year.ToString());
			s = s.Replace("%version%", EntryAssemblyAttrib("version"));
			s = s.Replace("%builddate%", EntryAssemblyAttrib("builddate"));
			return s;
		}

		// <summary>
		// things to do when form is loaded
		// </summary>
		private void AboutBox_Load(object sender, EventArgs e)
		{
			// if the user didn't provide an assembly, try to guess which one is the entry assembly
			if (_EntryAssembly == null){
				_EntryAssembly = Assembly.GetEntryAssembly();
			}
			if (_EntryAssembly == null){
				_EntryAssembly = Assembly.GetExecutingAssembly();
			}

			if (!MoreRichTextBox.Visible){
				Height -= MoreRichTextBox.Height;
			}
		}

		// <summary>
		// things to do when form is FIRST painted
		// </summary>
		private void AboutBox_Paint(object sender, PaintEventArgs e)
		{
			if (!_IsPainted){
				_IsPainted = true;
				Application.DoEvents();
				Cursor.Current = Cursors.WaitCursor;
				PopulateLabels();
				Cursor.Current = Cursors.Default;
			}
		}

		// <summary>
		// for detailed system info, launch the external Microsoft system info app
		// </summary>
		private void SysInfoButton_Click(object sender, EventArgs e)
		{
			ShowSysInfo();
		}

		// <summary>
		// launch any http:// or mailto: links clicked in the body of the rich text box
		// </summary>
		private void MoreRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				var sInfo = new ProcessStartInfo(e.LinkText) { UseShellExecute = true, };
				Process.Start(sInfo);
			}
			catch (Exception)
			{
			}
		}
	}
}