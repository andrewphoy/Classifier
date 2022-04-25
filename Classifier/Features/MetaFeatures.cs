using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classifier.Models;
using MachineLearning;

namespace Classifier.Features {
    public static class MetaFeatures {

        [Feature]
        public static int SolutionLength(LichessPuzzle puzzle) {
            return puzzle.MoveList.Split(' ').Length - 1;
        }

    }
}
