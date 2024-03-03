using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Common
{
    //TODO: do we need this?
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
