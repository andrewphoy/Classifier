using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning {
    public class MultiLabelRandomForest<TItem> : RandomForest<TItem> {

        public IEnumerable<string> PossibleLabels { get; set; }

        private Dictionary<string, List<DecisionTree<TItem, bool>>> Trees { get; set; }

        public void Evaluate(IEnumerable<TItem> items, Func<TItem, List<string>> desired) {
            int cntSuccess = 0;
            int cntFails = 0;
            int cntLabels = 0;
            int cntNonLabels = 0;
            int cntCorrectLabels = 0;
            int cntCorrectNonlabels = 0;

            //Console.WriteLine("Evaluation");

            foreach (var item in items) {
                foreach (var method in base.FeatureInitializers) {
                    method.Invoke(null, new object[] { item });
                }

                var labels = Evaluate(item);
                var correct = desired(item);
                foreach (var l in PossibleLabels) {
                    if (correct.Contains(l)) {
                        cntLabels++;
                    } else {
                        cntNonLabels++;
                    }

                    if (correct.Contains(l) && labels.Contains(l)) {
                        cntCorrectLabels++;
                        cntSuccess++;
                    } else if (!correct.Contains(l) && !labels.Contains(l)) {
                        cntCorrectNonlabels++;
                        cntSuccess++;
                    } else {
                        cntFails++;
                    }

                    //int falsePositives = cntNonLabels - cntCorrectNonlabels;
                    //Console.WriteLine($"{l}:\tLabels: {cntCorrectLabels} / {cntLabels}\t False Pos.: {falsePositives} / {cntNonLabels}");
                }
            }

            int cntTotal = cntSuccess + cntFails;
            Console.WriteLine($"Results: {cntSuccess} / {cntTotal} = {(double)cntSuccess / cntTotal}\tLabels: {cntCorrectLabels} / {cntLabels}\tNon Labels: {cntCorrectNonlabels} / {cntNonLabels}");
        }

        public void PrintFeaturesWithCounts() {
            foreach (var kvp in Trees) {
                Console.WriteLine(kvp.Key);

                Dictionary<string, int> featureUsage = new Dictionary<string, int>();
                Stack<DecisionTreeNode<TItem, bool>> stack = new Stack<DecisionTreeNode<TItem, bool>>();

                foreach (var tree in kvp.Value) {
                    stack.Push(tree.RootNode);

                    while (stack.Count > 0) {
                        var node = stack.Pop();
                        if (node.ChildNodes != null) {
                            foreach (var child in node.ChildNodes.Values) {
                                if (child != null) {
                                    stack.Push(child);
                                }
                            }
                        }
                        
                        if (!featureUsage.ContainsKey(node.FullName)) {
                            featureUsage[node.FullName] = 0;
                        }
                        featureUsage[node.FullName]++;
                    }
                }

                foreach (var feature in featureUsage.OrderByDescending(f => f.Value)) {
                    Console.WriteLine("\t" + feature.Value + "\t" + feature.Key);
                }
            }
        }

        public List<string> Evaluate(TItem item) {
            var labels = new List<string>();

            foreach (var label in PossibleLabels) {
                var trees = this.Trees[label];
                int cntYes = 0;
                int cntNo = 0;

                for (int i = 0; i < trees.Count; i++) {
                    if (trees[i].Evaluate(item, out bool success)) {
                        if (success) {
                            cntYes += trees[i].TreeWeight ?? 1;
                        } else {
                            cntNo += trees[i].TreeWeight ?? 1;
                        }
                    }
                }

                if (cntYes > 0 && cntYes >= cntNo) {
                    labels.Add(label);
                }
            }

            return labels;
        }

        public void Weight(IEnumerable<TItem> weightingSet, Func<TItem, IEnumerable<string>> existingLabels) {
            int cntWeightingSet = 0;
            foreach (var item in weightingSet) {
                foreach (var method in base.FeatureInitializers) {
                    method.Invoke(null, new object[] { item });
                }

                cntWeightingSet++;
                var desired = existingLabels(item);

                foreach (var label in PossibleLabels) {
                    foreach (var tree in Trees[label]) {
                        if (tree.Evaluate(item, out bool prediction)) {
                            if (prediction && desired.Contains(label)) {
                                tree.WeightingCorrect++;
                            } else if (!prediction && !desired.Contains(label)) {
                                tree.WeightingCorrect++;
                            } else {
                                tree.WeightingWrong++;
                            }
                        }
                    }
                }
            }

            foreach (var label in PossibleLabels) {
                int numTrees = Trees[label].Count;
                foreach (var tree in Trees[label]) {
                    double weight = (double)tree.WeightingCorrect / ((double)cntWeightingSet);
                    tree.TreeWeight = 1 + (int)Math.Round(weight * numTrees);
                }
            }
        }

        public void Train(IEnumerable<TItem> trainingSet, Func<TItem, IEnumerable<string>> existingLabels, int? randomSeed = null) {
            if (PossibleLabels == null || PossibleLabels.Count() == 0) {
                throw new ArgumentNullException("Must provide possible labels");
            }
            base.GetFeatures();

            if (!this.NumPredictors.HasValue || this.NumPredictors.Value < 1) {
                throw new ArgumentException("Invalid number of predictors");
            }

            if (this.NumEstimators <= 0) {
                throw new ArgumentOutOfRangeException("NumEstimators");
            }
            this.Trees = new Dictionary<string, List<DecisionTree<TItem, bool>>>();

            base.Random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

            foreach (string label in PossibleLabels) {
                this.Trees[label] = new List<DecisionTree<TItem, bool>>();

                List<TItem> positiveSet = trainingSet.Where(i => existingLabels(i).Contains(label)).ToList();
                List<TItem> negativeSet = trainingSet.Where(i => !existingLabels(i).Contains(label)).ToList();

                for (int i = 0; i < NumEstimators; i++) {
                    TrainTree(positiveSet, negativeSet, existingLabels, label);
                }
            }
        }

        private void TrainTree(List<TItem> positiveTrainingSet, List<TItem> negativeTrainingSet, Func<TItem, IEnumerable<string>> existingLabels, string label) {
            int halfSampleCount = base.NumSamples / 2;

            int cntPositive = positiveTrainingSet.Count;
            int cntNegative = negativeTrainingSet.Count;

            var sample = new List<TItem>();
            for (int i = 0; i < halfSampleCount; i++) {
                sample.Add(positiveTrainingSet[base.Random.Next(0, cntPositive)]);
                sample.Add(negativeTrainingSet[base.Random.Next(0, cntNegative)]);
            }

            this.Trees[label].Add(CreateTree(sample, existingLabels, label));
        }

        private DecisionTree<TItem, bool> CreateTree(IEnumerable<TItem> subset, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            // init the subset
            foreach (var item in subset) {
                foreach (var method in base.FeatureInitializers) {
                    method.Invoke(null, new object[] { item });
                }
            }

            var tree = new DecisionTree<TItem, bool>();
            tree.RootNode = GetNode(0, subset, existingLabels, targetLabel);

            return tree;
        }

        private DecisionTreeNode<TItem, bool> GetNode(int currentDepth, IEnumerable<TItem> subset, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            var features = this.Features.OrderBy(x => base.Random.NextDouble()).Take(base.NumPredictors.Value).ToList();
            
            // choose the feature with the lowest gini score

            double bestGini = double.MaxValue;
            DecisionTreeNode<TItem, bool> bestNode = null;

            foreach (var feature in features) {
                (var gini, var node) = GetSplit(feature, subset, existingLabels, targetLabel);

                if (gini < bestGini) {
                    bestGini = gini;
                    bestNode = node;

                    bestNode.FullName = feature.DeclaringType + "." + feature.Name;
                    bestNode.Name = feature.DeclaringType.Name + "." + feature.Name + " " + bestNode.Name;
                    bestNode.Depth = currentDepth;
                }
            }

            //int cntPositiveLeft = bestNode.LeftTrainingSet.Count(i => existingLabels(i).Contains(targetLabel));
            //int cntPositiveRight = bestNode.RightTrainingSet.Count(i => existingLabels(i).Contains(targetLabel));

            //Console.WriteLine($"Depth: {currentDepth} Node: {bestNode.Name} Score: {bestGini} Left: {bestNode.LeftTrainingSet.Count} Right: {bestNode.RightTrainingSet.Count}");
            //Console.WriteLine($"Left: {bestNode.LeftTrainingSet.Count(i => existingLabels(i).Contains(targetLabel))} Right: {bestNode.RightTrainingSet.Count(i => existingLabels(i).Contains(targetLabel))}");

            if (currentDepth == base.MaxDepth) {
                bestNode.IsTerminal = true;

                foreach (var kvp in bestNode.TrainingSets) {
                    int cntYes = 0;
                    int cntNo = 0;

                    foreach (var item in kvp.Value) {
                        if (existingLabels(item).Contains(targetLabel)) {
                            cntYes++;
                        } else {
                            cntNo++;
                        }
                    }

                    bestNode.Predictions[kvp.Key] = cntYes > 0 && cntYes >= cntNo;
                }

            } else {
                foreach (var kvp in bestNode.TrainingSets) {
                    int cntYes = 0;
                    int total = 0;
                    foreach (var item in kvp.Value) {
                        if (existingLabels(item).Contains(targetLabel)) {
                            cntYes++;
                        }
                        total++;
                    }

                    if (cntYes == 0) {
                        bestNode.ChildNodes[kvp.Key] = null;
                        bestNode.Predictions[kvp.Key] = false;
                    } else if (cntYes == total) {
                        bestNode.ChildNodes[kvp.Key] = null;
                        bestNode.Predictions[kvp.Key] = true;
                    } else {
                        bestNode.ChildNodes[kvp.Key] = GetNode(currentDepth + 1, kvp.Value, existingLabels, targetLabel);
                    }
                }
            }

            return bestNode;
        }

        private (double, DecisionTreeNode<TItem, bool>) GetSplit(MemberInfo feature, IEnumerable<TItem> subset, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            Type resultType;
            
            if (feature is PropertyInfo propertyInfo) {
                resultType = propertyInfo.PropertyType;
                if (resultType == typeof(int)) {
                    return GetIntegerSplit(item => (int)propertyInfo.GetValue(item), subset, existingLabels, targetLabel);
                } else if (resultType == typeof(bool)) {
                    return GetBooleanSplit(item => (bool)propertyInfo.GetValue(item), subset, existingLabels, targetLabel);
                } else if (resultType.IsEnum) {
                    return GetEnumSplit(item => propertyInfo.GetValue(item), subset, existingLabels, targetLabel);
                } else {
                    return (double.MaxValue, null);
                    //throw new NotImplementedException();
                }

            } else if (feature is MethodInfo methodInfo) {
                resultType = methodInfo.ReturnType;
                if (resultType == typeof(int)) {
                    return GetIntegerSplit(item => (int)methodInfo.Invoke(null, new object[] { item }), subset, existingLabels, targetLabel);
                } else if (resultType == typeof(bool)) {
                    return GetBooleanSplit(item => (bool)methodInfo.Invoke(null, new object[] { item }), subset, existingLabels, targetLabel);
                } else if (resultType.IsEnum) {
                    return GetEnumSplit(item => methodInfo.Invoke(null, new object[] { item }), subset, existingLabels, targetLabel);
                } else {
                    return (double.MaxValue, null);
                }
            }


            return (double.MaxValue, null);
        }

        private (double, DecisionTreeNode<TItem, bool>) GetIntegerSplit(Func<TItem, int> func, IEnumerable<TItem> subset, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {

            // iterate to find the split value
            HashSet<int> testedValues = new HashSet<int>();
            int bestValue = 0;
            double bestGini = double.MaxValue;
            DecisionTreeNode<TItem, bool> bestNode = null;

            foreach (var item in subset) {
                int val = func(item);
                if (testedValues.Contains(val)) {
                    continue;
                }

                var candidateNode = new DecisionTreeNode<TItem, bool>() {
                    Name = "Cutoff: " + val,
                    Predict = item => func(item) >= val
                };

                foreach (var other in subset) {
                    int otherVal = func(other);
                    bool greaterOrEqual = otherVal >= val;
                    candidateNode.AddTrainingItem(greaterOrEqual, other);
                }

                double gini = GetGiniIndex(candidateNode, existingLabels, targetLabel);
                if (gini < bestGini) {
                    bestValue = val;
                    bestGini = gini;
                    bestNode = candidateNode;
                }

                testedValues.Add(val);
            }

            return (bestGini, bestNode);
        }

        private (double, DecisionTreeNode<TItem, bool>) GetBooleanSplit(Func<TItem, bool> func, IEnumerable<TItem> subset, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            var node = new DecisionTreeNode<TItem, bool>() {
                Predict = item => func(item)
            };

            foreach (var item in subset) {
                bool val = func(item);
                node.AddTrainingItem(val, item);
            }

            return (GetGiniIndex(node, existingLabels, targetLabel), node);
        }
        
        private (double, DecisionTreeNode<TItem, bool>) GetEnumSplit(Func<TItem, object> func, IEnumerable<TItem> subset, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            var node = new DecisionTreeNode<TItem, bool>() {
                Predict = item => func(item)
            };

            foreach (var item in subset) {
                object val = func(item);
                node.AddTrainingItem(val, item);
            }

            return (GetGiniIndex(node, existingLabels, targetLabel), node);
        }

        private double GetGiniIndex(DecisionTreeNode<TItem, bool> node, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            int total = node.TrainingSets.Values.Sum(l => l.Count);
            double gini = 0;

            foreach (object key in node.TrainingSets.Keys) {
                gini += GetGiniIndex(node.TrainingSets[key], total, existingLabels, targetLabel);
            }

            return gini;
        }

        private double GetGiniIndex(List<TItem> items, int total, Func<TItem, IEnumerable<string>> existingLabels, string targetLabel) {
            if (items.Count == 0) {
                return 0;
            }

            double cntTrue = 0;
            double cntFalse = 0;
            foreach (var item in items) {
                bool hasLabel = existingLabels(item).Contains(targetLabel);
                if (hasLabel) {
                    cntTrue++;
                } else {
                    cntFalse++;
                }
            }

            var num = cntTrue / items.Count;
            var score = Math.Pow(cntTrue / items.Count, 2) + Math.Pow(cntFalse / items.Count, 2);

            return (1 - score) * ((double)items.Count / total);
        }
    }
}
