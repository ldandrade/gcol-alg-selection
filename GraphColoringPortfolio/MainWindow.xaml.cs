using GraphColoringPortfolio.Controllers;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphColoringPortfolio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ObservableCollection<String> graphInstancesFiles = new ObservableCollection<String>();
        private static ObservableCollection<String> graphInstancesFilenames = new ObservableCollection<String>();
        private const int MAXNTHREADS = 7;

        UInt16 algorithmsSelected;
        private static Object myLock = new Object();
        struct GCAlgorithmRun
        {
            public Process gcprogram;
            public String algorithm;
            public String filePath;
        }
        private static ConcurrentQueue<GCAlgorithmRun> executionQueue;
        private static bool consuming;
        private static CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
            algorithmsSelected = 0;
            executionQueue = new ConcurrentQueue<GCAlgorithmRun>();
            cts = new CancellationTokenSource();
            GCPortfolioDBDataContext context = new GCPortfolioDBDataContext();
            if (context.DatabaseExists())
            {
                foreach (GraphInstance graph in context.GraphInstances.OrderBy(graph => graph.Name))
                {
                    graphInstancesFilenames.Add(graph.Name);
                    graphInstancesFiles.Add(graph.Path);
                }
                lv_GraphInstances.ItemsSource = graphInstancesFilenames;
                lbl_NumberOfInstances.Content = graphInstancesFiles.Count.ToString() + " # Instances Loaded";
            }
            consuming = false;
        }

        //######################## BUTTON CLICK EVENTS ##############################################################################################//
        //Load List of Instances Filenames into ListView and Memory
        private void btn_LoadInstances_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "DIMACS COL files (*.col) | *.col";
            if (openFileDialog.ShowDialog() == true)
                foreach (string filename in openFileDialog.FileNames)
                {
                    graphInstancesFiles.Add(filename);
                    graphInstancesFilenames.Add(System.IO.Path.GetFileNameWithoutExtension(filename));

                    //Data maping object to our database
                    GCPortfolioDBDataContext context = new GCPortfolioDBDataContext();
                    GraphInstance graph = new GraphInstance();
                    graph.Id = Guid.NewGuid();
                    graph.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
                    graph.Path = filename;
                    //Adds an entity in a pending insert state to this System.Data.Linq.Table<TEntity>and parameter is the entity which to be added
                    context.GraphInstances.InsertOnSubmit(graph);
                    // executes the appropriate commands to implement the changes to the database
                    context.SubmitChanges();
                }
            lv_GraphInstances.ItemsSource = graphInstancesFilenames;
            lbl_NumberOfInstances.Content = graphInstancesFiles.Count.ToString() + " # Instances Loaded";
        }

        private void btn_CreateEdgeLists_Click(object sender, RoutedEventArgs e)
        {
            var options = new ParallelOptions();
            options.CancellationToken = cts.Token;
            options.MaxDegreeOfParallelism = MAXNTHREADS;
            try
            {
                Parallel.ForEach(graphInstancesFiles, options, (currentFile) =>
                {
                    using (var context = new GCPortfolioDBDataContext())
                    {
                        string line;
                        GraphInstance graph = context.GraphInstances.SingleOrDefault(existingGraph => existingGraph.Path == currentFile);
                        // Read the file and display it line by line.
                        System.IO.StreamReader fileReader = new System.IO.StreamReader(graph.Path);
                        System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(graph.Path + ".edl");
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            if (line.First().Equals('e'))
                                fileWriter.WriteLine(line.Remove(0, 2));
                        }
                        fileReader.Close();
                        fileWriter.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        //Gather Performance Metrics from selected Graph Coloring Algorithms
        private void btn_GatherMetrics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Checks all requirements before proceeding to graph coloring algorithm execution
                if (algorithmsSelected == 0)
                    throw new Exception("AlgorithmNotSelectedException");
                if (graphInstancesFiles.Count == 0)
                    throw new Exception("NoInstancesLoadedException");

                //Start threads for each graph coloring algorithm
                if ((bool)chk_BackTrackDSATUR.IsChecked)
                    produce("BacktrackingDSatur");
                if ((bool)chk_DSATUR.IsChecked)
                    produce("DSatur");
                if ((bool)chk_RandomGreedy.IsChecked)
                    produce("RandomGreedy");
                if ((bool)chk_RLF.IsChecked)
                    produce("RLF");
                if ((bool)chk_HillClimber.IsChecked)
                    produce("HillClimber");
                if ((bool)chk_AntCol.IsChecked)
                    produce("AntCol");
                if ((bool)chk_HEA.IsChecked)
                    produce("HEA");
                if ((bool)chk_PartialCol.IsChecked)
                    produce("PartialCol");
                if ((bool)chk_TabuCol.IsChecked)
                    produce("TabuCol");
                if (consuming)
                {
                    cts.Cancel();
                    cts = new CancellationTokenSource();
                }
                lbl_RemainingMetrics.Content = "Execution Queue: " + executionQueue.Count.ToString();
                Task.Factory.StartNew(() => consume());
            }
            catch (Exception ex)
            {
                if (ex.Message == "AlgorithmNotSelectedException")
                    MessageBox.Show("Please select at least one algorithm from the Algorithm Space.");
                if (ex.Message == "NoInstancesLoadedException")
                    MessageBox.Show("Please load the graph instances in the Problem Space.");
                else
                    MessageBox.Show(ex.ToString());
            }
        }

        //Create ARFF files according to the selected feature sets and learning methods
        private void btn_CreateARFF_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();
            String path = dialog.SelectedPath;

            if ((bool)chk_ClassificationAvgBest.IsChecked)
            {
                Dataset.setClassAttributeClassification("AvgBest");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Classification", "AvgBest", ""), "Classification", "AvgBest", "");
            }
            if ((bool)chk_ClassificationColorTime.IsChecked)
            {
                Dataset.setClassAttributeClassification("ColorTime");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Classification", "ColorTime", ""), "Classification", "ColorTime", "");
            }
            if ((bool)chk_ClassificationColorChecks.IsChecked)
            {
                Dataset.setClassAttributeClassification("ColorChecks");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Classification", "ColorChecks", ""), "Classification", "ColorChecks", "");
            }
            if ((bool)chk_MultiClassificationZero.IsChecked)
            {
                Dataset.setClassAttributeClassification("MultiLabel0");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Multi-Label-Classification", "Zero-Tolerance", ""), "Multi-Label-Classification-Zero", "", "");
            }
            if ((bool)chk_MultiClassificationFive.IsChecked)
            {
                Dataset.setClassAttributeClassification("MultiLabel5");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Multi-Label-Classification", "Five-Tolerance", ""), "Multi-Label-Classification-Five", "", "");
            }
            if ((bool)chk_MultiClassificationTen.IsChecked)
            {
                Dataset.setClassAttributeClassification("MultiLabel10");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Multi-Label-Classification", "Ten-Tolerance", ""), "Multi-Label-Classification-Ten", "", "");
            }
            if ((bool)chk_RegressionColor.IsChecked)
            {
                Dataset.setClassAttributeRegression("color");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "DSatur"), "Regression", "color", "DSatur");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "BacktrackingDSatur"), "Regression", "color", "BacktrackingDSatur");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "RandomGreedy"), "Regression", "color", "RandomGreedy");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "RLF"), "Regression", "color", "RLF");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "HEA"), "Regression", "color", "HEA");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "TabuCol"), "Regression", "color", "TabuCol");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "PartialCol"), "Regression", "color", "PartialCol");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "color", "AntCol"), "Regression", "color", "AntCol");
            }
            if ((bool)chk_RegressionTime.IsChecked)
            {
                Dataset.setClassAttributeRegression("time");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "DSatur"), "Regression", "time", "DSatur");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "BacktrackingDSatur"), "Regression", "time", "BacktrackingDSatur");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "RandomGreedy"), "Regression", "time", "RandomGreedy");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "RLF"), "Regression", "time", "RLF");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "HEA"), "Regression", "time", "HEA");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "TabuCol"), "Regression", "time", "TabuCol");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "PartialCol"), "Regression", "time", "PartialCol");
                ARFFIO.writeARFFContent(ARFFIO.writeARFFHeader(path, "Regression", "time", "AntCol"), "Regression", "time", "AntCol");
            }
        }

        //######################## CORE PROCEDURES ##############################################################################################//
        private void consume()
        {
            consuming = true;
            var options = new ParallelOptions();
            options.CancellationToken = cts.Token;
            options.MaxDegreeOfParallelism = MAXNTHREADS;
            try
            {
                Parallel.ForEach(executionQueue, options, q =>
                {
                    GCAlgorithmRun process;
                    if (executionQueue.TryDequeue(out process))
                    {
                        process.gcprogram.Start();
                        process.gcprogram.WaitForExit();
                        String retMessage = String.Empty;
                        retMessage = process.gcprogram.StandardOutput.ReadToEnd();
                        lock (myLock)
                        {
                            //parseLastResult(retMessage, process.algorithm, process.filePath);
                            parseResults(retMessage, process.algorithm, process.filePath);
                            lbl_RemainingMetrics.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                            {
                                lbl_RemainingMetrics.Content = "Execution Queue: " + executionQueue.Count.ToString();
                            }
                            ));
                        }
                        process.gcprogram.Close();
                    }
                    if (options.CancellationToken.IsCancellationRequested)
                        return;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            consuming = false;
        }

        private static void produce(String algorithm)
        {
            using (var context = new GCPortfolioDBDataContext())
            {
                List<string> graphInstancesFilesList = graphInstancesFiles.ToList();
                graphInstancesFilesList.Reverse();
                foreach (String currentFile in graphInstancesFilesList)
                {
                    String exec = String.Empty;
                    GraphInstance graph = context.GraphInstances.SingleOrDefault(existingGraph => existingGraph.Path == currentFile);
                    GColAlgorithmsRun gcolAlgRun = context.GColAlgorithmsRuns.Where(existingRun => existingRun.GraphID == graph.Id && existingRun.GColAlgorithm == algorithm).SingleOrDefault();
                    if (gcolAlgRun == null)
                        switch (algorithm)
                        {
                            case "BacktrackingDSatur":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\BacktrackingDSatur.exe";
                                break;
                            case "DSatur":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\DSatur.exe";
                                break;
                            case "RandomGreedy":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\SimpleGreedy.exe";
                                break;
                            case "RLF":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\RLF.exe";
                                break;
                            case "HillClimber":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\HillClimber.exe";
                                break;
                            case "AntCol":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\AntCol.exe";
                                break;
                            case "HEA":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\HybridEA.exe";
                                break;
                            case "PartialCol":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\PartialColAndTabuCol.exe";
                                break;
                            case "TabuCol":
                                exec = "C:\\Users\\Lucas\\SkyDrive\\Estudos\\UFRJ\\Dissertação\\GCP Algorithms\\gCol-LEWIS\\PartialColAndTabuCol.exe";
                                break;
                            default:
                                break;
                        }
                    //If there is work to be done, in other words, the metric is not present in the database
                    if (exec != String.Empty)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        Process p = new Process();

                        startInfo.CreateNoWindow = true;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardInput = true;
                        startInfo.UseShellExecute = false;
                        if (algorithm == "TabuCol")
                            startInfo.Arguments = currentFile + " -t -v -s 50000000000";
                        else if (algorithm == "DSatur" || algorithm == "RLF" || algorithm == "RandomGreedy")
                            startInfo.Arguments = currentFile + " -v";
                        else
                            startInfo.Arguments = currentFile + " -v -s 50000000000";
                        startInfo.FileName = exec;
                        p.StartInfo = startInfo;
                        GCAlgorithmRun run = new GCAlgorithmRun();
                        run.filePath = currentFile;
                        run.algorithm = algorithm;
                        run.gcprogram = p;
                        executionQueue.Enqueue(run);
                    }
                }
            }
        }

        //private static void parseLastResult(String result, String algorithm, String currentFile)
        //{
        //    bool existingMetric = true;
        //    //Data maping object to our database
        //    using (var context = new GCPortfolioDBDataContext())
        //    {

        //        //Get Single Graph which need to update
        //        GraphInstance graph = context.GraphInstances.Single(existingGraph => existingGraph.Path == currentFile);
        //        String numericResults = String.Empty;
        //        //Retrieves Performance Metrics
        //        if (result.LastIndexOf("\r\n\r\n") == -1)
        //            numericResults = result.Substring(result.IndexOf("COLS"), result.LastIndexOf("\r\n") - result.IndexOf("COLS"));
        //        else
        //            numericResults = result.Substring(result.IndexOf("COLS"), result.LastIndexOf("\r\n\r\n") - result.IndexOf("COLS"));
        //        long ccheck = Convert.ToInt64(numericResults.Substring(numericResults.LastIndexOf("\t")));
        //        long cputime = Convert.ToInt64(numericResults.Substring(numericResults.LastIndexOf(" "), numericResults.LastIndexOf("m") - numericResults.LastIndexOf(" ")));
        //        Int16 chromaticnumber = Convert.ToInt16(numericResults.Substring(numericResults.LastIndexOf("\n") + 2, numericResults.LastIndexOf(" ") - 2 - numericResults.LastIndexOf("\n") + 1));

        //        //Get Single metric which need to update. If it does not exist, create a new metric object for this graph instance
        //        PerformanceMetric metric = context.PerformanceMetrics.SingleOrDefault(gcMetric => gcMetric.GraphID == graph.Id);
        //        if (metric == null)
        //        {
        //            existingMetric = false;
        //            metric = new PerformanceMetric();
        //            metric.Id = Guid.NewGuid();
        //            metric.GraphID = graph.Id;
        //        }

        //        switch (algorithm)
        //        {
        //            case "BackTrack DSATUR":
        //                metric.BacktrackingDSaturConstraintChecks = ccheck;
        //                metric.BacktrackingDSaturCPUTime = cputime;
        //                metric.BacktrackingDSaturChromaticNumber = chromaticnumber;
        //                break;
        //            case "DSATUR":
        //                metric.DSaturConstraintChecks = ccheck;
        //                metric.DSaturCPUTime = cputime;
        //                metric.DSaturChromaticNumber = chromaticnumber;
        //                break;
        //            case "Random Greedy":
        //                metric.RandomGreedyConstraintChecks = ccheck;
        //                metric.RandomGreedyCPUTime = cputime;
        //                metric.RandomGreedyChromaticNumber = chromaticnumber;
        //                break;
        //            case "RLF":
        //                metric.RLFConstraintChecks = ccheck;
        //                metric.RLFCPUTime = cputime;
        //                metric.RLFChromaticNumber = chromaticnumber;
        //                break;
        //            case "HillClimber":
        //                metric.HillClimberConstraintChecks = ccheck;
        //                metric.HillClimberCPUTime = cputime;
        //                metric.HillClimberChromaticNumber = chromaticnumber;
        //                break;
        //            case "AntCol":
        //                metric.AntColConstraintChecks = ccheck;
        //                metric.AntColCPUTime = cputime;
        //                metric.AntColChromaticNumber = chromaticnumber;
        //                break;
        //            case "HEA":
        //                metric.HEAConstraintChecks = ccheck;
        //                metric.HEACPUTime = cputime;
        //                metric.HEAChromaticNumber = chromaticnumber;
        //                break;
        //            case "PartialCol":
        //                metric.PartialColConstraintChecks = ccheck;
        //                metric.PartialColCPUTime = cputime;
        //                metric.PartialColChromaticNumber = chromaticnumber;
        //                break;
        //            case "TabuCol":
        //                metric.TabuColConstraintChecks = ccheck;
        //                metric.TabuColCPUTime = cputime;
        //                metric.TabuColChromaticNumber = chromaticnumber;
        //                break;
        //            default:
        //                break;
        //        }

        //        //Adds an entity in a pending insert state to this System.Data.Linq.Table<TEntity>and parameter is the entity which to be added
        //        if (!existingMetric)
        //            context.PerformanceMetrics.InsertOnSubmit(metric);

        //        context.SubmitChanges();
        //    }
        //}


        private static void parseResults(String result, String algorithm, String currentFile)
        {
            //Data maping object to our database
            using (var context = new GCPortfolioDBDataContext())
            {
                GraphInstance graph = context.GraphInstances.Single(existingGraph => existingGraph.Path == currentFile);
                String numericResults = String.Empty;
                //Parse all Performance Metrics from the Graph Coloring Program results
                if (result.LastIndexOf("\r\n\r\n") == -1)
                    numericResults = result.Substring(result.IndexOf("COLS"), result.LastIndexOf("\r\n") - result.IndexOf("COLS")+2);
                else
                    numericResults = result.Substring(result.IndexOf("COLS"), result.LastIndexOf("\r\n\r\n") - result.IndexOf("COLS")+2);
                numericResults = numericResults.Substring(numericResults.IndexOf("\n"));
                int rowStartIndex = numericResults.IndexOf("\n");
                int rowEndIndex = numericResults.IndexOf("\r");
                while (rowEndIndex>rowStartIndex)
                {
                    string row = numericResults.Substring(rowStartIndex, rowEndIndex-rowStartIndex);
                    long ccheck = Convert.ToInt64(row.Substring(row.IndexOf("\t")));
                    long cputime = Convert.ToInt64(row.Substring(row.LastIndexOf(" "), row.LastIndexOf("m") - row.LastIndexOf(" ")));
                    int chromaticnumber = Convert.ToInt32(row.Substring(row.LastIndexOf("\n") + 2, row.LastIndexOf(" ") - 2 - row.LastIndexOf("\n") + 1));

                    //Create a new metric object for this graph instance
                    PerformanceMetrics2 metric = new PerformanceMetrics2();
                    metric.Id = Guid.NewGuid();
                    metric.GraphID = graph.Id;
                    metric.GColAlgorithm = algorithm;
                    metric.Checks = ccheck;
                    metric.Colors = chromaticnumber;
                    metric.Miliseconds = cputime;
                    context.PerformanceMetrics2s.InsertOnSubmit(metric);
                    context.SubmitChanges();

                    numericResults = numericResults.Remove(rowStartIndex, rowEndIndex+1);
                    rowStartIndex = numericResults.IndexOf("\n");
                    rowEndIndex = numericResults.IndexOf("\r");
                }
                //After all results are included in the database, the AlgorithmRun is considered completed.
                GColAlgorithmsRun gcolAlgRun = context.GColAlgorithmsRuns.Where(existingRun => existingRun.GraphID == graph.Id && existingRun.GColAlgorithm == algorithm).SingleOrDefault();
                if (gcolAlgRun == null)
                {
                    gcolAlgRun = new GColAlgorithmsRun();
                    gcolAlgRun.Id = Guid.NewGuid();
                    gcolAlgRun.GraphID = graph.Id;
                    gcolAlgRun.GColAlgorithm = algorithm;
                    gcolAlgRun.RunComplete = true;
                    gcolAlgRun.RunResult = result;
                    string totalTimeMs = result.Substring(result.LastIndexOf(" ") + 1, result.LastIndexOf("m") - result.LastIndexOf(" ") - 1);
                    gcolAlgRun.TotalTimeMiliSec = Convert.ToInt64(totalTimeMs);
                    context.GColAlgorithmsRuns.InsertOnSubmit(gcolAlgRun);
                    context.SubmitChanges();
                }
            }
        }



        //######################## CHECK AND UNCHECK EVENTS ##############################################################################################//

        private void chk_BackTrackDSATUR_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_BackTrackDSATUR_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_DSATUR_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_DSATUR_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_RandomGreedy_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_RandomGreedy_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_RLF_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_RLF_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_HillClimber_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_HillClimber_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_AntCol_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_AntCol_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_HEA_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_HEA_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_PartialCol_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }

        private void chk_PartialCol_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_TabuCol_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected--;
        }

        private void chk_TabuCol_Checked(object sender, RoutedEventArgs e)
        {
            algorithmsSelected++;
        }


        private void chk_Classification_Checked(object sender, RoutedEventArgs e)
        {
            chk_ClassificationAvgBest.IsChecked = true;
            chk_ClassificationColorTime.IsChecked = true;
            chk_ClassificationColorChecks.IsChecked = true;
        }

        private void chk_Classification_Unchecked(object sender, RoutedEventArgs e)
        {
            chk_ClassificationAvgBest.IsChecked = false;
            chk_ClassificationColorTime.IsChecked = false;
            chk_ClassificationColorChecks.IsChecked = false;
        }

        private void chk_MultiClassification_Checked(object sender, RoutedEventArgs e)
        {
            chk_MultiClassificationZero.IsChecked = true;
            chk_MultiClassificationFive.IsChecked = true;
            chk_MultiClassificationTen.IsChecked = true;
        }

        private void chk_MultiClassification_Unchecked(object sender, RoutedEventArgs e)
        {
            chk_MultiClassificationZero.IsChecked = false;
            chk_MultiClassificationFive.IsChecked = false;
            chk_MultiClassificationTen.IsChecked = false;
        }

        private void chk_Regression_Checked(object sender, RoutedEventArgs e)
        {
            chk_RegressionColor.IsChecked = true;
            chk_RegressionTime.IsChecked = true;
        }

        private void chk_Regression_Unchecked(object sender, RoutedEventArgs e)
        {
            chk_RegressionColor.IsChecked = false;
            chk_RegressionTime.IsChecked = false;
        }

    }
}
