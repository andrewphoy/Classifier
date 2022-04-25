using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning {
    internal class DecisionTreeNode<TItem, TPrediction> {
        public DecisionTreeNode() {
            this.Predictions = new Dictionary<object, TPrediction>();
            this.ChildNodes = new Dictionary<object, DecisionTreeNode<TItem, TPrediction>>();
            this.TrainingSets = new Dictionary<object, List<TItem>>();
        }

        public string FullName { get; set; }

        public string Name { get; set; }
        public int Depth { get; set; }
        public bool IsTerminal { get; set; }

        public Dictionary<object, TPrediction> Predictions { get; set; }
        public Dictionary<object, DecisionTreeNode<TItem, TPrediction>> ChildNodes { get; set; }
        public Dictionary<object, List<TItem>> TrainingSets { get; set; }

        public void AddTrainingItem(object key, TItem item) {
            if (!TrainingSets.ContainsKey(key)) {
                TrainingSets[key] = new List<TItem>();
            }
            TrainingSets[key].Add(item);
        }
        
        public Func<TItem, object> Predict { get; set; }
    }
}
