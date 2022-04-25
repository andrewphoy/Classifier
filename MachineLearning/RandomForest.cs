using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning {
    public class RandomForest<TItem> {
        /// <summary>
        /// The number of trees in the forest
        /// </summary>
        public int NumEstimators { get; set; }
        /// <summary>
        /// The number of samples to take for a single tree (with replacement)
        /// </summary>
        public int NumSamples { get; set; }
        /// <summary>
        /// The number of features to choose from at each node
        /// </summary>
        public int? NumPredictors { get; set; }
        /// <summary>
        /// The maximum depth of a single tree
        /// </summary>
        public int MaxDepth { get; set; }

        protected Random Random { get; set; }

        protected List<MemberInfo> Features { get; set; } = new List<MemberInfo>();
        protected List<MethodInfo> FeatureInitializers { get; set; }

        public IEnumerable<string> AvailableFeatures {
            get {
                foreach (var feature in Features) {
                    yield return feature.DeclaringType + "." + feature.Name;
                }
            }
        }

        protected void GetFeatures() {
            var type = typeof(TItem);

            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods());

            var methodFeatures = methods.Where(m => m.GetCustomAttribute<FeatureAttribute>(false) != null)
                .Where(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(TItem));

            var properties = type.GetProperties().Where(p => p.GetCustomAttribute<FeatureAttribute>(false) != null);

            this.Features.AddRange(methodFeatures);
            this.Features.AddRange(properties);

            this.FeatureInitializers = methods.Where(m => m.GetCustomAttribute<InitializeFeaturesAttribute>(false) != null)
                .Where(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(TItem)).ToList();

            if (!NumPredictors.HasValue) {
                NumPredictors = (int)Math.Ceiling(Math.Sqrt(this.Features.Count));
            }
        }
    }
}
