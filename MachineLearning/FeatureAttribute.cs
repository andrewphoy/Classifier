using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class FeatureAttribute : Attribute { }
}
