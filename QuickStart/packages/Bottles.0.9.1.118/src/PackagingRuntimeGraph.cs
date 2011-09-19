using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Assemblies;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles
{
    public class PackagingRuntimeGraph
    {
        private readonly IList<IActivator> _activators = new List<IActivator>();
        private readonly IAssemblyLoader _assemblies;
        private readonly IList<IBootstrapper> _bootstrappers = new List<IBootstrapper>();
        private readonly IPackagingDiagnostics _diagnostics;
        private readonly IList<IPackageLoader> _loaders = new List<IPackageLoader>();
        private readonly Stack<string> _provenanceStack = new Stack<string>();
        private readonly IList<IPackageInfo> _packages;

        public PackagingRuntimeGraph(IPackagingDiagnostics diagnostics, IAssemblyLoader assemblies, IList<IPackageInfo> packages)
        {
            _diagnostics = diagnostics;
            _assemblies = assemblies;
        	_packages = packages;
        }


        public void PushProvenance(string provenance)
        {
            _provenanceStack.Push(provenance);
        }

        public void PopProvenance()
        {
            _provenanceStack.Pop();
        }

        public void DiscoverAndLoadPackages(Action onAssembliesScanned, bool runActivators = true)
        {
            findAllPackages();

            loadAssemblies(_packages, onAssembliesScanned);
            var discoveredActivators = runAllBootstrappers();

            if(runActivators)
            {
                activatePackages(_packages, discoveredActivators);    
            }
        }

        private void activatePackages(IList<IPackageInfo> packages, IList<IActivator> discoveredActivators)
        {
            var discoveredPlusRegisteredActivators = discoveredActivators.Union(_activators);
            _diagnostics.LogExecutionOnEach(discoveredPlusRegisteredActivators,
                                            activator => activator.Activate(packages, _diagnostics.LogFor(activator)));
        }

        public void AddBootstrapper(IBootstrapper bootstrapper)
        {
            _bootstrappers.Add(bootstrapper);
            _diagnostics.LogObject(bootstrapper, currentProvenance);
        }

        public void AddLoader(IPackageLoader loader)
        {
            _loaders.Add(loader);
            _diagnostics.LogObject(loader, currentProvenance);
        }

        public void AddActivator(IActivator activator)
        {
            _activators.Add(activator);
            _diagnostics.LogObject(activator, currentProvenance);
        }

        public void AddFacility(IPackageFacility facility)
        {
            _diagnostics.LogObject(facility, currentProvenance);

            PushProvenance(facility.ToString());
            
            facility.As<IPackagingRuntimeGraphConfigurer>().Configure(this);
            PopProvenance();
        }

        private string currentProvenance
        {
            get { return _provenanceStack.Peek(); }
        }

        private List<IActivator> runAllBootstrappers()
        {
            var discoveredActivators = new List<IActivator>();
            _diagnostics.LogExecutionOnEach(_bootstrappers, b =>
            {
                var bootstrapperActivators = b.Bootstrap(_diagnostics.LogFor(b));
                discoveredActivators.AddRange(bootstrapperActivators);
                _diagnostics.LogBootstrapperRun(b, bootstrapperActivators);
            });
            return discoveredActivators;
        }

        private void loadAssemblies(IEnumerable<IPackageInfo> packages, Action onAssembliesScanned)
        {
            _diagnostics.LogExecutionOnEach(packages, _assemblies.ReadPackage);

            onAssembliesScanned();
        }

        private void findAllPackages()
        {
            _diagnostics.LogExecutionOnEach(_loaders, loader =>
            {
                var log = _diagnostics.LogFor(loader);
                var packageInfos = loader.Load(log).ToArray();
                _diagnostics.LogPackages(loader, packageInfos);
                _packages.AddRange(packageInfos);
            });
        }
    }
}