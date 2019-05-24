using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Terminals
{
    internal static partial class Program
    {
        /// <summary>
        ///     Represents the program assembly info object.
        /// </summary>
        internal static class Info
        {
            private static readonly Assembly aAssembly = Assembly.GetExecutingAssembly();

            private static string aboutText;

            /// <summary>
            ///     Gets full path to the executing assembly location without last backslash
            /// </summary>
            internal static string Location => Path.GetDirectoryName(aAssembly.Location);

            public static string DLLVersion => aAssembly.GetName().Version.ToString();

            public static DateTime BuildDate => RetrieveLinkerTimestamp();

            public static string Description
            {
                get
                {
                    var desc = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(aAssembly,
                        typeof(AssemblyDescriptionAttribute));
                    return desc.Description;
                }
            }

            public static string Title
            {
                get
                {
                    var title = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(aAssembly,
                        typeof(AssemblyTitleAttribute));
                    return title.Title;
                }
            }

            public static string TitleVersion => $"{Title} {VersionString}";

            public static string VersionString { get; private set; }

            public static Version Version => aAssembly.GetName().Version;

            /// <summary>
            ///     Taken from http://stackoverflow.com/questions/1600962/c-displaying-the-build-date
            ///     (code by Joe Spivey)
            /// </summary>
            private static DateTime RetrieveLinkerTimestamp()
            {
                var filePath = aAssembly.Location;
                const int c_PeHeaderOffset = 60;
                const int c_LinkerTimestampOffset = 8;
                var b = new byte[2048];
                Stream s = null;

                try
                {
                    s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    s.Read(b, 0, 2048);
                }
                finally
                {
                    s?.Close();
                }

                var i = BitConverter.ToInt32(b, c_PeHeaderOffset);
                var secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
                var dt = new DateTime(1970, 1, 1, 0, 0, 0);
                dt = dt.AddSeconds(secondsSince1970);
                dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
                return dt;
            }

            internal static void SetApplicationVersion()
            {
                //MAJOR.MINOR.PATCH.BUILD
                //MAJOR == Breaking Changes in API or features
                //MINOR == Non breaking changes, but significant feature changes
                //PATH (Or Revision) == Bug fixes only, etc...
                //BUILD == Build increments
                //
                //Incremental builds, daily, etc will include full M.M.P.B
                //Release builds only include M.M.P
                //
                //.NET Likes:  MAJOR.MINOR.BUILD.REVISION

                var version = Assembly.GetExecutingAssembly().GetName().Version;
                VersionString = $"{version.Major}.{version.Minor}.{version.Build}";
                aboutText = TitleVersion;
                SetDebugBuild(version);
            }

            [Conditional("DEBUG")]
            private static void SetDebugBuild(Version version)
            {
                // debug builds, to keep track of minor/revisions, etc..
                // Adds also the revision
                VersionString = version.ToString();
                aboutText = $"{TitleVersion} - {BuildDate.ToShortDateString()}";
            }

            internal static string GetAboutText(string persistenceId)
            {
                return $"{aboutText} ({persistenceId} store)";
            }
        }
    }
}