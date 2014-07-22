using System;
using System.Linq;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Execution
{
    public class ExecutionPlan
    {
        readonly BehaviorChain<ClassExecution> classBehaviors;

        public ExecutionPlan(Configuration config)
        {
            classBehaviors =
                BuildClassBehaviorChain(config,
                    BuildInstanceBehaviorChain(config,
                        BuildCaseBehaviorChain(config)));
        }

        public void ExecuteClassBehaviors(ClassExecution classExecution)
        {
            classBehaviors.Execute(classExecution);
        }

        static BehaviorChain<ClassExecution> BuildClassBehaviorChain(Configuration config, BehaviorChain<InstanceExecution> instanceBehaviors)
        {
            var chain = config.CustomClassBehaviors
                .Select(customBehavior => (ClassBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(GetInnermostBehavior(config, instanceBehaviors));

            return new BehaviorChain<ClassExecution>(chain);
        }

        static BehaviorChain<InstanceExecution> BuildInstanceBehaviorChain(Configuration config, BehaviorChain<CaseExecution> caseBehaviors)
        {
            var chain = config.CustomInstanceBehaviors
                .Select(customBehavior => (InstanceBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new ExecuteCases(caseBehaviors));

            return new BehaviorChain<InstanceExecution>(chain);
        }

        static BehaviorChain<CaseExecution> BuildCaseBehaviorChain(Configuration config)
        {
            var chain = config.CustomCaseBehaviors
                .Select(customBehavior => (CaseBehavior)Activator.CreateInstance(customBehavior))
                .ToList();

            chain.Add(new InvokeMethod());

            return new BehaviorChain<CaseExecution>(chain);
        }

        static ClassBehavior GetInnermostBehavior(Configuration config, BehaviorChain<InstanceExecution> instanceBehaviors)
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(config.TestClassFactory, instanceBehaviors);

            return new CreateInstancePerClass(config.TestClassFactory, instanceBehaviors);
        }
    }
}