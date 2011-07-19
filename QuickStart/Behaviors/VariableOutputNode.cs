using System;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;

namespace QuickStart.Behaviors
{
    public class VariableOutputNode : OutputNode
    {
        private readonly IList<OutputHolder> _outputs = new List<OutputHolder>();

        public VariableOutputNode() : base(typeof(RenderVariableOutput))
        {}

        public void AddOutput(Func<OutputFormatDetector, bool> isMatch, OutputNode output)
        {
            _outputs.Add(new OutputHolder(isMatch, output));
        }

        protected override void configureObject(ObjectDef def)
        {
            ObjectDef currentCandidate = null;
            foreach (var pair in _outputs.Reverse())
            {
                var candidate = new ObjectDef(typeof(ConditionalOutput));
                candidate.DependencyByValue(typeof(Func<OutputFormatDetector, bool>), pair.Predicate);
                addDependency<IActionBehavior>(candidate, pair.OutputNode);
                if (currentCandidate != null)
                {
                    addDependency<ConditionalOutput>(candidate, currentCandidate);
                }
                currentCandidate = candidate;
            }

            addDependency<ConditionalOutput>(def, currentCandidate);
        }

        private static void addDependency<T>(ObjectDef item, IContainerModel dependency)
        {
            addDependency<T>(item, dependency.ToObjectDef());
        }

        private static void addDependency<T>(ObjectDef item, ObjectDef dependency)
        {
            item.Dependency(typeof(T), dependency);
        }

        class OutputHolder
        {
            public Func<OutputFormatDetector, bool> Predicate { get; private set; }
            public OutputNode OutputNode { get; private set; }

            public OutputHolder(Func<OutputFormatDetector, bool> predicate, OutputNode outputNode)
            {
                Predicate = predicate;
                OutputNode = outputNode;
            }
        }
    }


}