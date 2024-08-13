using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Dotnet.Link
{
	[DebuggerDisplay("{Identity}")]
	public class ProjectItem : Dictionary<string, string>
	{
		public string Identity
		{
			get => GetRequired();
			set => Set(value);
		}
		public string FullPath
		{
			get => GetRequired();
			set => Set(value);
		}
		public string RootDir
		{
			get => GetRequired();
			set => Set(value);
		}
		public string Filename
		{
			get => GetRequired();
			set => Set(value);
		}
		public string Extension
		{
			get => GetRequired();
			set => Set(value);
		}
		public string? RelativeDir
		{
			get => Get();
			set => Set(value);
		}
		public string? Directory
		{
			get => Get();
			set => Set(value);
		}

		public bool IsKeyOutput => bool.TryParse(Get(), out bool value) && value;
		public bool Pack => bool.TryParse(Get(), out bool value) && value;

		public string? PackagePath
		{
			get => Get();
			set => Set(value);
		}

		public string? RecursiveDir
		{
			get => Get();
			set => Set(value);
		}
		public string? ModifiedTime
		{
			get => Get();
			set => Set(value);
		}
		public string? CreatedTim
		{
			get => Get();
			set => Set(value);
		}
		public string? AccessedTime
		{
			get => Get();
			set => Set(value);
		}
		public string? DefiningProjectFullPath
		{
			get => Get();
			set => Set(value);
		}
		public string? DefiningProjectDirectory
		{
			get => Get();
			set => Set(value);
		}
		public string? DefiningProjectName
		{
			get => Get();
			set => Set(value);
		}
		public string? DefiningProjectExtension
		{
			get => Get();
			set => Set(value);
		}


		private string? Get([CallerMemberName] string memberName = "")
			=> ContainsKey(memberName) ? this[memberName] : null;

		private string GetRequired([CallerMemberName] string memberName = "")
			=> this[memberName];


		private void Set(string? value, [CallerMemberName] string memberName = "")
		{
			if (value is null)
			{
				Remove(memberName);
			}
			else
			{
				this[memberName] = value;
			}
		}
	}
}
