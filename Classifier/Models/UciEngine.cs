using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Classifier.Models {
    public class UciEngine : IDisposable {

        private readonly Process _process;
        private SemaphoreSlim _syncStart = new SemaphoreSlim(0, 1);
        private SemaphoreSlim _syncReady = new SemaphoreSlim(0, 1);
        //private SemaphoreSlim _syncParse = new SemaphoreSlim(1, 1);
        private SemaphoreSlim _syncAnalyzing = new SemaphoreSlim(0, 1);

        public bool Running { get; private set; }
        public EngineEval CurrentEval { get; private set; }

        public UciEngine(string uciPath) {
            ProcessStartInfo si = new ProcessStartInfo {
                FileName = uciPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            _process = new Process {
                StartInfo = si
            };

            _process.ErrorDataReceived += new DataReceivedEventHandler(Process_ErrorDataReceived);
            _process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);

        }

        public async Task Start() {
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            await SendLineAsync("uci");
            await _syncStart.WaitAsync();

            await SendLineAsync("isready");
            await _syncReady.WaitAsync();
        }

        public void Dispose() {
            try {
                if (Running) {
                    SendLineAsync("stop").Wait();
                }
                SendLineAsync("quit").Wait();
                _process.WaitForExit();
            } catch { }
            // todo kill other child processes

            try {
                if (!_process.HasExited) {
                    _process.Close();
                    _process.Kill();
                }
            } catch { }

            try {
                _process.Dispose();
            } catch { }
        }

        #region ReadOnly Properties
        public string Name { get; private set; }
        public string Author { get; private set; }
        public bool IsAlive {
            get {
                try {
                    return !_process.HasExited;
                } catch {
                    this.Dispose();
                    return false;
                }
            }
        }
        //public int Depth { get { return _depth; } }
        //public int Nodes { get { return _cntNodes; } }
        //public int Time { get { return _elapsedTime; } }
        //public float Score { get { return myPVs[0].RawEval; } }
        //public EngineLine[] PVs { get { return myPVs; } }
        #endregion

        #region Start/Stop
        //public async Task Start(string startCommand, int? nodes = null) {
        //    if (this.Running) {
        //        this.Stop();
        //    }

        //    this.Running = true;
        //    await SendLineAsync(startCommand);
        //    if (nodes.HasValue && nodes.Value > 0) {
        //        await SendLineAsync($"go nodes={nodes.Value}");
        //    } else {
        //        await SendLineAsync("go infinite");
        //    }
        //}

        public async Task StopAsync() {
            await SendLineAsync("stop");
            Running = false;
        }
        #endregion

        #region Sending Data
        private async Task SendLineAsync(string command) {
            //Console.WriteLine("[UCI Send] " + command);
            await _process.StandardInput.WriteLineAsync(command);
            await _process.StandardInput.FlushAsync();
        }
        #endregion

        #region Receiving Data
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (Debugger.IsAttached) { Debugger.Break(); }
        }

        private object _lockOutputData = new object();
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            lock (_lockOutputData) {
                ParseReceivedLine(e.Data);
            }
        }

        private void ParseReceivedLine(string line) {
            try {
                string[] words = line.Split(new char[] { '\t', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0) {
                    return;
                }
                ParseWords(words);
            } catch { }
        }

        /*
info depth 20 seldepth 30 multipv 1 score cp 76 nodes 3501156 nps 1309822 hashfull 899 tbhits 0 time 2673 pv d2d4 d7d5 g1f3 g8f6 c2c4 e7e6 b1c3 c7c5 c4d5 e6d5 c1g5 f8e7 d4c5 e8g8 e2e3 e7c5 f1d3 c8e6 e1g1 b8c6 a2a3 c5d6 c3b5
info depth 20 seldepth 29 multipv 2 score cp 70 nodes 3501156 nps 1309822 hashfull 899 tbhits 0 time 2673 pv e2e4 e7e6 d2d4 d7d5 b1c3 f8b4 e4d5 e6d5 f1d3 b8c6 g1f3 g8f6 d1e2 c8e6 a2a3 b4c3 b2c3 e8g8 a1b1
info depth 20 seldepth 29 multipv 3 score cp 67 nodes 3501156 nps 1309822 hashfull 899 tbhits 0 time 2673 pv e2e3 g8f6 g1f3 d7d5 c2c4 e7e6 f1e2 c7c5 d2d4 c5d4 e3d4 b8c6 b1c3 f8e7 e1g1 e8g8 a2a3 c8d7 f3e5 d5c4 c1f4 c6e5 d4e5
info depth 20 seldepth 25 multipv 4 score cp 61 nodes 3501156 nps 1309822 hashfull 899 tbhits 0 time 2673 pv g1f3 e7e6 d2d4 c7c5 e2e3 g8f6 c2c4 d7d5 f1d3 f8d6 b1c3 e8g8 e1g1 b8c6 c4d5 e6d5 d4c5 d6c5 c1d2 f8e8 a1c1 a7a6 a2a3 c5d6
bestmove d2d4 ponder d7d5
        */


        private void ParseWords(string[] words) {
            if (words.Length == 0) { return; }

            switch (words[0].ToLower()) {
                case "id":
                    if (words.Length > 3) {
                        if (words[1].ToLower() == "name") {
                            this.Name = string.Join(" ", words.Skip(2));
                        } else if (words[1].ToLower() == "author") {
                            this.Author = string.Join(" ", words.Skip(2));
                        }
                    }
                    break;

                case "uciok":
                    _syncStart.Release();
                    break;

                case "readyok":
                    _syncReady.Release();
                    break;

                case "bestmove":
                    if (CurrentEval != null && words.Length >= 2) {
                        CurrentEval.BestMove = words[1];
                    }
                    _syncAnalyzing.Release();
                    break;

                case "copyprotection":
                    break;

                case "registration":
                    break;

                case "info":
                    ExtractInfo(words.Skip(1).ToArray());
                    break;

                case "option":
                    break;

                default:
                    // not sure what to do, try the remaining words
                    if (words.Length > 1) {
                        ParseWords(words.Skip(1).ToArray());
                    } else {
                        // no more words, quietly ignore
                    }
                    break;
            }
        }

        private void ExtractInfo(string[] words) {
            int i = 0;

            string linePV = string.Empty;
            bool hasLine = false;
            int intResult;
            long longResult;

            EngineInfo info = new EngineInfo();


            bool done = false;
            while (!done) {
                switch (words[i].ToLower()) {
                    case "depth":
                        if (int.TryParse(words[i + 1], out intResult)) {
                            info.Depth = intResult;
                        }
                        i += 2;
                        break;

                    case "seldepth":
                        if (int.TryParse(words[i + 1], out intResult)) {
                            info.Seldepth = intResult;
                        }
                        i += 2;
                        break;

                    case "time":
                        if (long.TryParse(words[i + 1], out longResult)) {
                            info.ElapsedMs = longResult;
                        }
                        i += 2;
                        break;

                    case "nodes":
                        if (long.TryParse(words[i + 1], out longResult)) {
                            info.Nodes = longResult;
                        }
                        i += 2;
                        break;

                    case "pv":
                        hasLine = true;
                        info.Variation = string.Join(" ", words.Skip(i + 1));
                        done = true;
                        break;

                    case "multipv":
                        hasLine = true;
                        info.MultiPv = int.Parse(words[i + 1]);
                        i += 2;
                        break;

                    case "score":
                        if (int.TryParse(words[i + 2], out intResult)) {
                            if (string.Compare(words[i + 1], "mate", true) == 0) {
                                info.ScoreMate = intResult;
                            } else if (string.Compare(words[i + 1], "cp", true) == 0) {
                                info.ScoreCp = intResult;
                            } else {
                                info.ScoreCp = intResult;
                            }
                        }
                        i += 3;
                        break;

                    case "nps":
                        if (long.TryParse(words[i + 1], out longResult)) {
                            info.NodesPerSecond = longResult;
                        }
                        i += 2;
                        break;

                    case "tbhits":
                        if (int.TryParse(words[i + 1], out intResult)) {
                            info.TableBaseHits = intResult;
                        }
                        i += 2;
                        break;

                    case "hashfull":
                        if (int.TryParse(words[i + 1], out intResult)) {
                            info.HashFull = intResult;
                        }
                        i += 2;
                        break;

                    case "string":
                        done = true;
                        break;

                    default:
                        if (hasLine) {
                            linePV += " " + words[i].Trim();
                        }
                        i++;
                        break;
                }

                done = done || i >= words.Length;
            }

            if (info.MultiPv > 0) {
                CurrentEval.Variations[info.MultiPv] = info;
            }
        }

        #endregion

        public async Task UciNewGame() {
            await SendLineAsync("ucinewgame");
        }

        public async Task SetFenPosition(string fen) {
            await SendLineAsync("position fen " + fen);
        }

        public async Task<EngineEval> Analyze(int? nodes = null) {
            CurrentEval = new EngineEval();

            if (nodes.HasValue && nodes.Value > 0) {
                await SendLineAsync($"go nodes {nodes.Value}");
            } else {
                await SendLineAsync("go infinite");
            }

            await _syncAnalyzing.WaitAsync();
            return CurrentEval;
        }

        public async Task SetOption(string name, string value) {
            await SendLineAsync($"setoption name {name} value {value}");
        }
    }
}
