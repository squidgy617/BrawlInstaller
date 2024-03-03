using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Common
{
    /// <summary>
    /// Class responsible for creating composition of classes through dependency injection (using MEF)
    /// </summary>
    public static class CompositionBootstrapper
    {
        public static CompositionContainer Container { get; private set; }

        public static void InitializeContainer(object initPoint)
        {
            // All classes we want to compose are within the executing assembly - inside of this project
            var catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            Container = new CompositionContainer(catalog);

            // Resolves all imports and importingcontructors
            Container.SatisfyImportsOnce(initPoint);
        }

        public static void Dispose()
        {
            Container.Dispose();
        }
    }
}
