using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning {
    public class DecisionTree<TItem, TPrediction> {
        internal DecisionTreeNode<TItem, TPrediction> RootNode { get; set; }

        public int? TreeWeight { get; set; } = null;
        internal int WeightingCorrect { get; set; }
        internal int WeightingWrong { get; set; }

        public bool Evaluate(TItem item, out TPrediction prediction) {
            prediction = default(TPrediction);
            var node = RootNode;
            do {
                object nextKey = node.Predict(item);
                if (node.Predictions.ContainsKey(nextKey)) {
                    prediction = node.Predictions[nextKey];
                    return true;
                }
                if (node.ChildNodes.ContainsKey(nextKey)) {
                    node = node.ChildNodes[nextKey];
                } else {
                    return false;
                }
            } while (!node.IsTerminal);
            
            return false;
        }
    }
}
