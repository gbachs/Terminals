using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Terminals.Data;
using Terminals.Integration.Import.RdcMan;

namespace Terminals.Integration.Import
{
    internal class Importers : Integration<IImport>
    {
        private readonly IPersistence persistence;

        public Importers(IPersistence persistence)
        {
            this.persistence = persistence;
        }

        internal string GetProvidersDialogFilter()
        {
            this.LoadProviders();

            var stringBuilder = new StringBuilder();
            // work with copy because it is modified
            var extraImporters = new Dictionary<string, IImport>(this.providers);
            this.AddTerminalsImporter(extraImporters, stringBuilder);

            foreach (var importer in extraImporters)
                this.AddProviderFilter(stringBuilder, importer.Value);

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Forces terminals importer to be on first place
        /// </summary>
        private void AddTerminalsImporter(Dictionary<string, IImport> extraImporters, StringBuilder stringBuilder)
        {
            if (extraImporters.ContainsKey(ImportTerminals.TERMINALS_FILEEXTENSION))
            {
                var terminalsImporter = extraImporters[ImportTerminals.TERMINALS_FILEEXTENSION];
                this.AddProviderFilter(stringBuilder, terminalsImporter);
                extraImporters.Remove(ImportTerminals.TERMINALS_FILEEXTENSION);
            }
        }

        /// <summary>
        ///     Loads a new collection of favorites from source file.
        ///     The newly created favorites aren't imported into configuration.
        /// </summary>
        internal List<FavoriteConfigurationElement> ImportFavorites(string Filename)
        {
            var importer = this.FindProvider(Filename);

            if (importer == null)
                return new List<FavoriteConfigurationElement>();

            return importer.ImportFavorites(Filename);
        }

        internal List<FavoriteConfigurationElement> ImportFavorites(string[] files)
        {
            var favorites = new List<FavoriteConfigurationElement>();
            foreach (var file in files)
                favorites.AddRange(this.ImportFavorites(file));
            return favorites;
        }

        protected override void LoadProviders()
        {
            if (this.providers == null)
            {
                this.providers = new Dictionary<string, IImport>();
                this.providers.Add(ImportTerminals.TERMINALS_FILEEXTENSION, new ImportTerminals(this.persistence));
                this.providers.Add(ImportRDP.FILE_EXTENSION, new ImportRDP());
                this.providers.Add(ImportvRD.FILE_EXTENSION, new ImportvRD(this.persistence));
                this.providers.Add(ImportMuRD.FILE_EXTENSION, new ImportMuRD());
                this.providers.Add(ImportRdcMan.FILE_EXTENSION, new ImportRdcMan(this.persistence));
            }
        }

        /// <summary>
        ///     Disabled because of performance, there is no need to search all libraries,
        ///     because importers are implemented only in Terminals
        /// </summary>
        private void LoadImportersFromAssemblies()
        {
            if (this.providers == null)
            {
                this.providers = new Dictionary<string, IImport>();
                var dir = new DirectoryInfo(Program.Info.Location);
                string[] patterns = {"*.dll", "*.exe"};

                foreach (var pattern in patterns)
                foreach (var assemblyFile in dir.GetFiles(pattern))
                    this.LoadAssemblyImporters(assemblyFile.FullName);
            }
        }

        private void LoadAssemblyImporters(string assemblyFileFullName)
        {
            try
            {
                var assembly = Assembly.LoadFile(assemblyFileFullName);
                if (assembly != null)
                    this.LoadAssemblyImporters(assembly);
            }
            catch (Exception exc) //do nothing
            {
                Logging.Error("Error loading Assembly from Bin Folder" + assemblyFileFullName, exc);
            }
        }

        private void LoadAssemblyImporters(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                this.LoadAssemblyImporter(type);
        }

        private void LoadAssemblyImporter(Type type)
        {
            try
            {
                if (typeof(IImport).IsAssignableFrom(type) && type.IsClass)
                {
                    var importer = type.Assembly.CreateInstance(type.FullName) as IImport;
                    this.AddImporter(importer);
                }
            }
            catch (Exception exc)
            {
                Logging.Error("Error iterating Assemblies for Importer Classes", exc);
            }
        }

        private void AddImporter(IImport importer)
        {
            if (importer != null)
            {
                var extension = importer.KnownExtension.ToLower();
                if (this.ShouldAddImporterExtension(extension))
                    this.providers.Add(extension, importer);
            }
        }

        private bool ShouldAddImporterExtension(string extension)
        {
            return !string.IsNullOrEmpty(extension) &&
                   !this.providers.ContainsKey(extension);
        }
    }
}